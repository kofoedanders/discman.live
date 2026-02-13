\echo 'ETL users'
SET search_path TO disclive_development, public;

insert into public.users (
  id,
  username,
  password,
  salt,
  email,
  discman_points,
  elo,
  simple_scoring,
  emoji,
  country,
  register_put_distance,
  settings_initialized,
  last_email_sent,
  friends,
  news_ids_seen
)
select
  (d.data->>'Id')::uuid as id,
  d.data->>'Username' as username,
  decode(d.data->>'Password', 'base64') as password,
  decode(d.data->>'Salt', 'base64') as salt,
  coalesce(d.data->>'Email', '') as email,
  coalesce((d.data->>'DiscmanPoints')::int, 0) as discman_points,
  coalesce((d.data->>'Elo')::double precision, 1500) as elo,
  coalesce((d.data->>'SimpleScoring')::boolean, false) as simple_scoring,
  d.data->>'Emoji' as emoji,
  d.data->>'Country' as country,
  coalesce((d.data->>'RegisterPutDistance')::boolean, false) as register_put_distance,
  coalesce((d.data->>'SettingsInitialized')::boolean, false) as settings_initialized,
  (d.data->>'LastEmailSent')::timestamptz as last_email_sent,
  coalesce(array(select jsonb_array_elements_text(d.data->'Friends')), '{}') as friends,
  coalesce(array(select jsonb_array_elements_text(d.data->'NewsIdsSeen')), '{}') as news_ids_seen
from mt_doc_user d;

\echo 'ETL user rating history'

insert into public.user_ratings (
  "UserId",
  elo,
  date_time
)
select
  (u.data->>'Id')::uuid as user_id,
  (r->>'Elo')::double precision as elo,
  (r->>'DateTime')::timestamptz as date_time
from mt_doc_user u
cross join lateral jsonb_array_elements(coalesce(u.data->'RatingHistory', '[]'::jsonb)) r;

\echo 'Users source/target counts'
select count(*) as source_users from mt_doc_user;
select count(*) as target_users from public.users;
