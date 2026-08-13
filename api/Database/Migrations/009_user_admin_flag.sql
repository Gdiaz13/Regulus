-- Admin rights live on the user row as a single boolean. It defaults to false
-- so every existing and future account is an ordinary user until someone is
-- deliberately promoted.
alter table users
    add column if not exists is_admin boolean not null default false;

create index if not exists ix_users_is_admin on users (is_admin) where is_admin;
