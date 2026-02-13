\echo 'ETL tournaments'
SET search_path TO disclive_development, public;

insert into public.tournaments (
  id,
  name,
  created_at,
  start,
  "end",
  players,
  admins,
  courses,
  prices
)
select
  (t.data->>'Id')::uuid as id,
  t.data->>'Name' as name,
  (t.data->>'CreatedAt')::timestamptz as created_at,
  (t.data->>'Start')::timestamptz as start,
  (t.data->>'End')::timestamptz as end,
  coalesce(array(select jsonb_array_elements_text(t.data->'Players')), '{}') as players,
  coalesce(array(select jsonb_array_elements_text(t.data->'Admins')), '{}') as admins,
  coalesce(array(select (jsonb_array_elements_text(t.data->'Courses'))::uuid), '{}'::uuid[]) as courses,
  t.data->'Prices' as prices
from mt_doc_tournament t;

\echo 'Tournaments source/target counts'
select count(*) as source_tournaments from mt_doc_tournament;
select count(*) as target_tournaments from public.tournaments;
