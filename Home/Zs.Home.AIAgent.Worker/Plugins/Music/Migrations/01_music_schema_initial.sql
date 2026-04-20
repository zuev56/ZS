create schema if not exists music;

create table music.tracks (
    id              serial primary key,
    file_path       text not null unique,
    search_text     text not null,
    search_vector   tsvector,
    updated_at      timestamp with time zone default now()
);

create index idx_track_search_vector on music.tracks using GIN (search_vector);
