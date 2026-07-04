-- TCG prices are not one number: a sold price for a PSA 9 is not a listed price
-- for a raw card. These columns keep those apart. Provider stock rows leave them null.
alter table price_history add column if not exists price_type varchar(16) null;
alter table price_history add column if not exists card_condition varchar(32) null;
alter table price_history add column if not exists grade varchar(16) null;
alter table price_history add column if not exists currency varchar(8) null;
