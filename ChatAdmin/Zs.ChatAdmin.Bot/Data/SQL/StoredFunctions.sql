
-- Process incoming messages of specific chat
CREATE OR REPLACE FUNCTION ca.sf_process_group_message(
    _chat_id integer,
    _message_id integer,
    _accounting_start_date timestamp with time zone, -- важно переопределять во время выполнения
    _msg_limit_hi integer,                           -- важно переопределять во время выполнения
    _msg_limit_hihi integer,                         -- важно переопределять во время выполнения
    _msg_limit_after_ban integer = 5,
    _start_account_after integer default 100)        -- важно переопределять во время выполнения
    RETURNS json 
    LANGUAGE 'plpgsql'
AS $BODY$
DECLARE
   _user_id integer;
   _accounted_user_msg_count integer;
   _daily_chat_msg_count integer;
   _ban_id integer;
BEGIN
    
    select user_id into _user_id from bot.messages where id = _message_id;

    select id into _ban_id from ca.bans 
    where user_id = _user_id and chat_id = _chat_id
      and insert_date > now()::date -- - interval '3 hours' -- Захватываем баны с предыдущего дня
    order by insert_date desc limit 1;
      
    -- Если для пользователя есть активный бан, то удаляем сообщение. Учитываем бан с предыдущего дня
    if exists (select 1 from ca.bans 
                where id = _ban_id
                  and finish_date > now() 
             order by insert_date desc)
    then
        return '{
                    "Action": "DeleteMessage",
                    "Info": "Для пользователя имеется активный бан"
                }';
    end if;

-- !!! На случай, если было разорвано соединение с интернетом, и бан пользователя 
--     закончился к моменту его восстановления, то, несмотря на сброшенную дату начала учёта
--     ориентируемся на кол-во сообщений пользователя, оставленных после окончания бана
    if (select insert_date::date from ca.bans where id = _ban_id) = now()::date -- Важно учитывать только баны текущего дня
    then
        if (select count(m.*)
              from bot.messages m
         left join ca.bans b on b.id = _ban_id
             where m.insert_date > b.finish_date
               and m.user_id = _user_id
               and m.is_deleted = false) >= _msg_limit_after_ban
        then
            update ca.bans set finish_date = now()::date + interval '1 day' - interval '1 second'
            where id = _ban_id;
            return '{
                        "Action": "SendMessageToGroup", 
                        "MessageText" : "<UserName>, вы израсходовали свой лимит сообщений до конца дня",
                        "BanId": ' || _ban_id::text || '
                    }';
        end if;
    end if;
 
    -- Начало индивидуального учёта после _start_account_after сообщений в чате 
    -- от любых пользователей с 00:00 текущего дня
    _daily_chat_msg_count = (select count(*) from bot.messages where chat_id = _chat_id and insert_date > now()::date);
    if (_accounting_start_date is not null and _daily_chat_msg_count < _start_account_after)
    then
        return '{ 
                    "Action": "SetAccountingStartDate", 
                    "AccountingStartDate": null 
                }';
    end if;
 
    -- Дата начала учёта хранится в пямяти программы и передаётся в этот метод
    -- Переопределяется после перезагрузки или восстановления соединения с сетью
    -- Чтобы исключить массовое удаление сообщений, накопленных за время отключения бота от сети,
    -- нельзя хранить дату начала учёта в БД, как это планировалось изначально
    if (_accounting_start_date is null and _daily_chat_msg_count >= _start_account_after) 
    then
        return '{ 
                    "Action": "SetAccountingStartDate",
                    "AccountingStartDate": "' || to_char(now()::timestamp, 'YYYY-MM-DD"T"HH24:MI:SS"Z"') || E'"\n' ||',
                    "MessageText" : "В чате уже ' || _daily_chat_msg_count::text || ' сообщений. Начинаю персональный учёт." 
                }';
    elsif (_accounting_start_date is null and _daily_chat_msg_count < _start_account_after)
    then
        return '{ 
                    "Action": "Continue",
                    "Info": "Учёт сообщений ещё не начался" 
                }';
    end if;

    --select user_id into _user_id from bot.messages where id = _message_id;
     
    select count(*) into _accounted_user_msg_count from bot.messages 
    where insert_date >= _accounting_start_date 
      and user_id = _user_id
      and is_deleted = false;
  

    -- С начала учёта каждому доступно максимум _msg_limit_hihi сообщений.
    -- После _msg_limit_hi сообщения с начала учёта надо выдать пользователю 
    --     предупреждение о приближении к лимиту. При этом создаётся запись
    --     в таблице ca.bans и ставится пометка о том, что пользователь предупреждён
    if (_accounted_user_msg_count < _msg_limit_hi) then
        return '{ 
                     "Action": "Continue",
                     "Info": "Количество учтённых сообщений пользователя меньше предупредитетельной уставки: ' || _accounted_user_msg_count::text || ' < ' || _msg_limit_hi::text || '"
                }';
    elsif (_accounted_user_msg_count >= _msg_limit_hi and _accounted_user_msg_count < _msg_limit_hihi) then

        -- Создаём неактивную запись в таблице банов (без даты окончания), 
        -- выдаём предупреждение и фиксируем это, чтоб не повторяться
        if (_ban_id is null) then        
            insert into ca.bans (user_id, chat_id)
            select _user_id, _chat_id;
            
            select id into _ban_id from ca.bans 
             where user_id = _user_id and chat_id = _chat_id
               and insert_date > now()::date 
          order by insert_date desc limit 1;

            return '{
                        "Action": "SendMessageToGroup", 
                        "MessageText": "<UserName>, количеcтво сообщений, отправленных Вами с начала учёта: ' || _accounted_user_msg_count::text || '.\nОсталось сообщений до временного мьюта: ' || (_msg_limit_hihi - _accounted_user_msg_count)::text || '",
                        "BanId": ' || _ban_id::text || '
                    }';
        else
            return '{ 
                         "Action": "Continue",
                         "Info": "Предупреждение о приближении к лимиту было выслано ранее"
                    }';
        end if;
    elsif (_accounted_user_msg_count >= _msg_limit_hihi) then
        if (_ban_id is null) then
            return '{ 
                        "Action": "SendMessageToOwner", 
                        "MessageText": "Error! The user has exceeded the limit and I don''t know what to do!" 
                    }';
 
        -- Если бан не активен, активируем его (задаём дату окончания)
        -- Отправляем сообщение пользователю
        elsif (select finish_date from ca.bans where id = _ban_id) is null then
            update ca.bans set finish_date = now() + interval '3 hours'
            where id = _ban_id;
            return '{
                        "Action": "SendMessageToGroup", 
                        "MessageText": "<UserName>, Вы превысили лимит сообщений (' || _msg_limit_hihi::text || '). Все последующие сообщения в течение 3-х часов будут удаляться.\nПотом до конца дня у Вас будет ' || _msg_limit_after_ban::text || ' сообщений.",
                        "BanId": ' || _ban_id::text || '
                    }';

        -- Иначе, если бан активен и функция всё ещё выполняется, значит бан отработал 
        -- и у пользователя осталось _msg_limit_after_ban сообщений.
        elsif (_accounted_user_msg_count >= _msg_limit_hihi and _accounted_user_msg_count < _msg_limit_hihi + _msg_limit_after_ban) then
            return '{ 
                         "Action": "Continue",
                         "Info": "Бан закончился, пользователь расходует последние ' || _msg_limit_after_ban::text || ' сообщений за день"
                    }';

        -- При достижении второго предела отодвигаем время бана на конец дня и шлём предупреждающее сообщение
        elsif (_accounted_user_msg_count >= _msg_limit_hihi + _msg_limit_after_ban) then
            update ca.bans set finish_date = now()::date + interval '1 day' - interval '1 second'
            where id = _ban_id;
            return '{
                        "Action": "SendMessageToGroup", 
                        "MessageText" : "<UserName>, вы израсходовали свой лимит сообщений до конца дня",
                        "BanId": ' || _ban_id::text || '
                    }';
        else
            return '{ 
                        "Action": "SendMessageToOwner",
                        "Info" : "Error! The user has exceeded the limit but no condition has been met!" 
                    }';
        end if;
    end if;
    return '{
                "Action": "SendMessageToOwner",
                "Info" : "Error! End of function has been reached!" 
            }';
END;
$BODY$;
ALTER FUNCTION ca.sf_process_group_message(integer, integer, timestamp with time zone, integer, integer, integer, integer)
    OWNER TO postgres;




-- Get statistics of all chats in the specified time interval
CREATE OR REPLACE FUNCTION ca.sf_cmd_get_full_statistics(
    _users_limit integer,
    _from_date   timestamp with time zone,
    _to_date     timestamp with time zone DEFAULT now()
    )
    RETURNS text
    LANGUAGE 'plpgsql'
AS $BODY$
DECLARE
    _result_text         text = ''; 
    _header_active_users text = 'Active users:';   
    _header_total_msg    text = 'Total messages:';
    _chat_name           text;
    _chat_id             integer;
    _notice_item record;
BEGIN

  CREATE TEMP TABLE statisticsTable AS  
  SELECT chat_id
       , chat_name
       , user_id
       , row_header
       , all_message_count
       , accounted_message_count
    FROM
    (    
         -- All chats, all users, theirs all message count and message count from accounting date
        (SELECT s1.chat_id
              , c.name as chat_name
              , s1.user_id
              , coalesce(u.full_name, u.name) as row_header
              , s1.message_count as all_message_count
              , s2.message_count as accounted_message_count
           FROM bot.chats c
      LEFT JOIN bot.sf_get_chat_statistics(c.id, _users_limit, _from_date, _to_date) s1 ON s1.chat_id = c.id
--WARNING: ca.accountings не заполняется и не используется. Планируется её заполнение и использование только для отчётности
      LEFT JOIN bot.sf_get_chat_statistics(c.id, _users_limit, (SELECT start_date FROM ca.accountings WHERE start_date > _from_date LIMIT 1)
                 , _to_date) s2 ON s1.user_id = s2.user_id AND s1.chat_id = s2.chat_id
      LEFT JOIN bot.users u on u.id = s1.user_id
          WHERE s1.chat_id IS NOT NULL
       ORDER BY s1.chat_id, s1.message_count DESC, s2.message_count DESC)
              
    UNION
         -- Number of active users in chat
         SELECT s.chat_id
              , s.chat_name AS chat_name
              , null
              , _header_active_users   AS row_header
              , count(*)    AS all_message_count
              , NULL        AS accounted_message_count
           FROM (SELECT c.name as chat_name, c.id as chat_id
                   FROM bot.messages m
              LEFT JOIN bot.chats c ON c.id = m.chat_id
                  WHERE m.insert_date > _from_date AND m.insert_date < _to_date
                    AND m.is_deleted = false
                    AND c.chat_type_id = 'GROUP'
               GROUP BY m.user_id, c.id) s
       GROUP BY s.chat_id, s.chat_name
       
    UNION      
         -- Total messages in chat
         SELECT c.id as chat_id
              , c.name as chat_name--coalesce(u.full_name, c.chat_name)
              , null
              , _header_total_msg AS row_header
              , count(m.user_id)  AS all_message_count
              , NULL              AS accounted_message_count
           FROM bot.messages m
      LEFT JOIN bot.chats c ON c.id = m.chat_id
      LEFT JOIN bot.users u ON u.id = m.user_id
          WHERE m.insert_date > _from_date AND m.insert_date < _to_date
            AND (c.chat_type_id = 'GROUP' OR (c.chat_type_id = 'PRIVATE' AND u.is_bot = false)) -- remove bot's answers
            AND m.is_deleted = false
       GROUP BY c.id--, u.full_name
   ) full_query;

   --RAISE NOTICE '0.0. _result_text: %', _result_text;
   --RAISE NOTICE '0.1. statisticsTable: %', (select json_agg(q) from (select * from statisticsTable) q);
   
   
  -- Для каждого чата формируем сводку
  FOR _chat_id IN (SELECT chat_id FROM statisticsTable GROUP BY chat_id)
  LOOP
      _result_text = _result_text || '**' || (SELECT name FROM bot.chats WHERE id = _chat_id LIMIT 1) || E'**\n';
      _result_text = coalesce(_result_text, '') || coalesce((SELECT _header_total_msg || ' ' || all_message_count FROM statisticsTable WHERE chat_id = _chat_id and row_header = _header_total_msg), '');
      
      --RAISE NOTICE '1.0. _result_text: %', _result_text;
      --RAISE NOTICE '1.1. _chat_id: %', _chat_id;
      --RAISE NOTICE '1.2. _chat_name: %', _chat_name;
   
      -- Для групповых чатов больше информации
      IF EXISTS (SELECT 1 FROM statisticsTable WHERE chat_id = _chat_id and row_header = _header_active_users) 
      THEN
          _result_text = _result_text || E'\n' || (SELECT row_header || ' ' || all_message_count FROM statisticsTable WHERE chat_id = _chat_id and row_header = _header_active_users) || E'\n';
          _result_text = _result_text || E'---\nMost active users:\n';
          _result_text = _result_text || coalesce((SELECT string_agg(row_header || ' ' || all_message_count || COALESCE(' (' || accounted_message_count || '*)', ''), E'\n'
                                                                   ORDER BY all_message_count DESC, coalesce(accounted_message_count, 0) DESC) 
                                                   FROM statisticsTable
                                                  WHERE chat_id = _chat_id and row_header not in (_header_active_users, _header_total_msg)), '[exception]');
      END IF;

      --RAISE NOTICE '2. _result_text: %', _result_text;

      _result_text = coalesce(_result_text || E'\n\n================\n\n', '');
  END LOOP;
  
   _result_text = (SELECT trim(trailing E'\n\n================\n\n' from _result_text));

  DROP TABLE statisticsTable;
  --RAISE NOTICE '3. _result_text: %', _result_text;
  
  IF (_result_text is null or length(trim(_result_text)) = 0) THEN
      RETURN 'There are no messages in the specified time range';
  END IF;
  
  RETURN _result_text;
END;
$BODY$;
ALTER FUNCTION ca.sf_cmd_get_full_statistics(integer, timestamp with time zone, timestamp with time zone)
    OWNER TO postgres;
COMMENT ON FUNCTION ca.sf_cmd_get_full_statistics(integer, timestamp with time zone, timestamp with time zone)
    IS 'Returns all chats statistics in the specified time range';
--select ca.sf_cmd_get_full_statistics(15, '2020-10-31 00:00:00', '2020-10-31 23:59:59')


    
    
CREATE OR REPLACE FUNCTION ca.sf_get_most_popular_words(
    _chat_id integer,
    from_date timestamp with time zone,
    to_date timestamp with time zone DEFAULT now(),
    min_word_length integer DEFAULT 2)
    RETURNS TABLE(word character varying, count bigint) 
    LANGUAGE 'plpgsql'
    ROWS 1000
AS $BODY$
DECLARE
   msg_text text;
   words text[];
BEGIN
   msg_text = (select string_agg(m.raw_data ->> 'Text', ' ')
                 from bot.messages m
                where m.insert_date > from_date and m.insert_date < to_date
                  and m.chat_id = _chat_id);
                              
   msg_text = REPLACE(msg_text, chr(10), ' ');  
   msg_text = REPLACE(msg_text, '\\', ' ');
   msg_text = REPLACE(msg_text, '\n', ' ');
   msg_text = REPLACE(msg_text, '/', ' ');
   msg_text = REPLACE(msg_text, ',', ' ');
   msg_text = REPLACE(msg_text, '.', ' ');
   msg_text = REPLACE(msg_text, '(', ' ');
   msg_text = REPLACE(msg_text, ')', ' ');
   msg_text = REPLACE(msg_text, '[', ' ');
   msg_text = REPLACE(msg_text, ']', ' ');
   msg_text = REPLACE(msg_text, '{', ' ');
   msg_text = REPLACE(msg_text, '}', ' ');
   msg_text = REPLACE(msg_text, '?', ' ');
   msg_text = REPLACE(msg_text, '!', ' ');
   msg_text = REPLACE(msg_text, '"', ' ');
   msg_text = REPLACE(msg_text, '«', ' ');
   msg_text = REPLACE(msg_text, '»', ' ');
   msg_text = REPLACE(msg_text, '  ', ' ');
   msg_text = REPLACE(msg_text, '  ', ' ');
   msg_text = REPLACE(msg_text, '  ', ' '); 
   msg_text = REPLACE(msg_text, ' - ', ' ');
   msg_text = REPLACE(msg_text, '"
"', ' ' );
   
   words = string_to_array(LOWER(msg_text), ' ');

    RETURN QUERY(
        SELECT REPLACE(REPLACE(w::varchar(100), '(', '' ), ')', '')::varchar(100) as word, count(*) as word_count
        FROM (select unnest(words)) as w
        where Length(w::varchar(100)) >= min_word_length + 2
          and w::varchar(100) not in (SELECT ('(' || id || ')') FROM ca.auxiliary_words)
        group by w
        having count(*) > 1
        order by word_count desc
    );
END;
$BODY$;
ALTER FUNCTION ca.sf_get_most_popular_words(integer, timestamp with time zone, timestamp with time zone, integer)
    OWNER TO postgres;


     

CREATE OR REPLACE FUNCTION ca.sf_get_bans(
    _chat_id integer,
    _from_date timestamp with time zone,
    _to_date timestamp with time zone DEFAULT now())
    RETURNS TABLE(user_name character varying, status character varying) 
    LANGUAGE 'plpgsql'
    ROWS 1000
AS $BODY$
DECLARE
   msg_text text;
   words text[];
BEGIN

    RETURN QUERY(
        SELECT 
            coalesce(u.full_name, u.name) as user_name,
            case when finish_date is null then 'Warned'::varchar else 'Banned'::varchar end as status
        FROM ca.bans b
        LEFT JOIN bot.users u ON u.id = b.user_id
        WHERE b.chat_id = _chat_id 
        AND b.insert_date > _from_date AND b.insert_date < _to_date
        ORDER BY b.insert_date
    );
END;
$BODY$;
ALTER FUNCTION ca.sf_get_bans(integer, timestamp with time zone, timestamp with time zone)
    OWNER TO postgres;




CREATE OR REPLACE FUNCTION ca.sf_job_get_yesterdays_statistics(_chat_id integer)
RETURNS text
    LANGUAGE 'plpgsql'
AS $BODY$
DECLARE
BEGIN
    RETURN (SELECT string_agg(function_results.val, ' ') as Result
           FROM (
               SELECT * FROM (
                   SELECT ca.sf_cmd_get_full_statistics(15, now()::Date - interval '1 day', now()) as val, 1 as order
                   UNION
                   SELECT chr(10) || chr(10) || 'The most popular words:' || chr(10) || string_agg(w.word || ' (' || w.count || ')', ',  ') as val, 2 as order
                   FROM (SELECT * FROM ca.sf_get_most_popular_words(_chat_id, now()::Date - interval '1 day', now(), 2) LIMIT 10) w
                   UNION
                   SELECT chr(10) || chr(10) || 'Bans and warnings:' || chr(10) || string_agg(user_name || ' - ' || status, chr(10)) as val, 3 as order
                   FROM (SELECT * FROM ca.sf_get_bans(_chat_id, now()::Date - interval '1 day', now())) b
               ) r
           ORDER BY r.order
           ) function_results);
END;
$BODY$;
ALTER FUNCTION ca.sf_job_get_yesterdays_statistics(integer)
    OWNER TO postgres;
COMMENT ON FUNCTION ca.sf_job_get_yesterdays_statistics(integer)
    IS 'Returns data for ChatAdminBot''s job';
--select ca.sf_job_get_yesterdays_statistics(1)

