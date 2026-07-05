-- Persist the original prediction signals beside each scored outcome so model
-- evaluation can compare confidence, risk, bias, and horizon against reality.
alter table model_accuracy_results add column if not exists confidence_score double precision not null default 0;
alter table model_accuracy_results add column if not exists risk_score double precision not null default 0;
alter table model_accuracy_results add column if not exists bullish_score double precision not null default 0;
alter table model_accuracy_results add column if not exists bearish_score double precision not null default 0;
alter table model_accuracy_results add column if not exists time_horizon_days integer not null default 0;

create index if not exists ix_model_accuracy_results_model_horizon
    on model_accuracy_results (model_name, time_horizon_days, scored_at desc);
