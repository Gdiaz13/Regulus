-- Attach existing global rows to one disabled legacy owner, then make new app
-- data explicitly user-owned through the authenticated user's id.
insert into users (id, email, normalized_email, display_name, password_hash, is_active)
values (
    '00000000-0000-0000-0000-000000000001',
    'legacy@local.regulas',
    'LEGACY@LOCAL.REGULAS',
    'Legacy Local Data',
    '',
    false
)
on conflict (id) do nothing;

alter table stocks add column if not exists user_id uuid;
update stocks set user_id = '00000000-0000-0000-0000-000000000001' where user_id is null;
alter table stocks alter column user_id set not null;
alter table stocks add constraint fk_stocks_user_id foreign key (user_id) references users (id) on delete cascade;

drop index if exists ux_stocks_symbol;
create unique index if not exists ux_stocks_user_symbol on stocks (user_id, symbol);
create index if not exists ix_stocks_user_id on stocks (user_id);

alter table comments add column if not exists user_id uuid;
update comments
set user_id = stocks.user_id
from stocks
where comments.stock_id = stocks.id and comments.user_id is null;
alter table comments alter column user_id set not null;
alter table comments add constraint fk_comments_user_id foreign key (user_id) references users (id) on delete cascade;
create index if not exists ix_comments_user_id on comments (user_id);

alter table predictions add column if not exists user_id uuid;
update predictions set user_id = '00000000-0000-0000-0000-000000000001' where user_id is null;
alter table predictions alter column user_id set not null;
alter table predictions add constraint fk_predictions_user_id foreign key (user_id) references users (id) on delete cascade;
create index if not exists ix_predictions_user_asset_created_on on predictions (user_id, asset_id, created_on desc);
