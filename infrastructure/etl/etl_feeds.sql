\echo 'ETL global feed items'
SET search_path TO disclive_development, public;

insert into public.global_feed_items (
  "Id",
  item_type,
  achievement_name,
  registered_at,
  subjects,
  course_name,
  hole_score,
  hole_number,
  round_scores,
  action,
  likes,
  round_id,
  tournament_id,
  tournament_name,
  friend_name
)
select
  (g.data->>'Id')::uuid as id,
  coalesce((g.data->>'ItemType')::int, 0) as item_type,
  g.data->>'AchievementName' as achievement_name,
  coalesce((g.data->>'RegisteredAt')::timestamptz, '0001-01-01 00:00:00+00'::timestamptz) as registered_at,
  coalesce(array(select jsonb_array_elements_text(
    case
      when jsonb_typeof(g.data->'Subjects') = 'array' then g.data->'Subjects'
      else '[]'::jsonb
    end
  )), '{}') as subjects,
  g.data->>'CourseName' as course_name,
  coalesce((g.data->>'HoleScore')::int, 0) as hole_score,
  coalesce((g.data->>'HoleNumber')::int, 0) as hole_number,
  coalesce(array(select (jsonb_array_elements_text(
    case
      when jsonb_typeof(g.data->'RoundScores') = 'array' then g.data->'RoundScores'
      else '[]'::jsonb
    end
  ))::int), '{}'::int[]) as round_scores,
  coalesce((g.data->>'Action')::int, 0) as action,
  coalesce(array(select jsonb_array_elements_text(
    case
      when jsonb_typeof(g.data->'Likes') = 'array' then g.data->'Likes'
      else '[]'::jsonb
    end
  )), '{}') as likes,
  coalesce(nullif(g.data->>'RoundId', '')::uuid, '00000000-0000-0000-0000-000000000000'::uuid) as round_id,
  coalesce(nullif(g.data->>'TournamentId', '')::uuid, '00000000-0000-0000-0000-000000000000'::uuid) as tournament_id,
  g.data->>'TournamentName' as tournament_name,
  g.data->>'FriendName' as friend_name
from mt_doc_globalfeeditem g;

\echo 'ETL user feed items'

insert into public.user_feed_items (
  "Id",
  username,
  feed_item_id,
  item_type,
  registered_at
)
select
  (u.data->>'Id')::uuid as id,
  u.data->>'Username' as username,
  coalesce(nullif(u.data->>'FeedItemId', '')::uuid, '00000000-0000-0000-0000-000000000000'::uuid) as feed_item_id,
  coalesce((u.data->>'ItemType')::int, 0) as item_type,
  coalesce((u.data->>'RegisteredAt')::timestamptz, '0001-01-01 00:00:00+00'::timestamptz) as registered_at
from mt_doc_userfeeditem u;

\echo 'Feeds source/target counts'
select count(*) as source_global_feed from mt_doc_globalfeeditem;
select count(*) as target_global_feed from public.global_feed_items;
select count(*) as source_user_feed from mt_doc_userfeeditem;
select count(*) as target_user_feed from public.user_feed_items;
