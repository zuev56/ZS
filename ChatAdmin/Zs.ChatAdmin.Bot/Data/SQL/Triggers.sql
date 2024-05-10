CREATE TRIGGER accountings_reset_update_date
    BEFORE UPDATE 
    ON ca.accountings
    FOR EACH ROW
    EXECUTE FUNCTION public.reset_update_date();

CREATE TRIGGER bans_reset_update_date
    BEFORE UPDATE 
    ON ca.bans
    FOR EACH ROW
    EXECUTE FUNCTION public.reset_update_date();

CREATE TRIGGER notifications_reset_update_date
    BEFORE UPDATE 
    ON ca.notifications
    FOR EACH ROW
    EXECUTE FUNCTION public.reset_update_date();