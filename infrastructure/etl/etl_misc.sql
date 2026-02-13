\echo 'ETL reset password requests'
SET search_path TO disclive_development, public;

insert into public.reset_password_requests (
  "Id",
  email,
  username,
  created_at
)
select
  (r.data->>'Id')::uuid as id,
  r.data->>'Email' as email,
  r.data->>'Username' as username,
  coalesce((r.data->>'CreatedAt')::timestamptz, '0001-01-01 00:00:00+00'::timestamptz) as created_at
from mt_doc_resetpasswordrequest r;

\echo 'Reset password requests source/target counts'
select count(*) as source_reset from mt_doc_resetpasswordrequest;
select count(*) as target_reset from public.reset_password_requests;
