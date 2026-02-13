\echo 'ETL rounds'
SET search_path TO disclive_development, public;

insert into public.rounds (
  id,
  spectators,
  score_mode,
  round_name,
  course_name,
  course_layout,
  course_id,
  start_time,
  is_completed,
  completed_at,
  created_by,
  achievements,
  deleted
)
select
  (r.data->>'Id')::uuid as id,
  coalesce(array(select jsonb_array_elements_text(r.data->'Spectators')), '{}') as spectators,
  coalesce((r.data->>'ScoreMode')::int, 0) as score_mode,
  r.data->>'RoundName' as round_name,
  r.data->>'CourseName' as course_name,
  r.data->>'CourseLayout' as course_layout,
  (r.data->>'CourseId')::uuid as course_id,
  (r.data->>'StartTime')::timestamptz as start_time,
  coalesce((r.data->>'IsCompleted')::boolean, false) as is_completed,
  (r.data->>'CompletedAt')::timestamptz as completed_at,
  coalesce(r.data->>'CreatedBy', '') as created_by,
  r.data->'Achievements' as achievements,
  coalesce((r.data->>'Deleted')::boolean, false) as deleted
from mt_doc_round r;

\echo 'ETL round signatures'

insert into public.player_signatures (
  "RoundId",
  username,
  base64_signature,
  signed_at
)
select
  (r.data->>'Id')::uuid as round_id,
  s->>'Username' as username,
  s->>'Base64Signature' as base64_signature,
  (s->>'SignedAt')::timestamptz as signed_at
from mt_doc_round r
cross join lateral jsonb_array_elements(coalesce(r.data->'Signatures', '[]'::jsonb)) s;

\echo 'ETL round rating changes'

insert into public.rating_changes (
  "RoundId",
  username,
  change
)
select
  (r.data->>'Id')::uuid as round_id,
  rc->>'Username' as username,
  (rc->>'Change')::double precision as change
from mt_doc_round r
cross join lateral jsonb_array_elements(coalesce(r.data->'RatingChanges', '[]'::jsonb)) rc;

\echo 'ETL player scores'

insert into public.player_scores (
  "RoundId",
  player_name,
  player_emoji,
  player_round_status_emoji,
  course_average_at_the_time,
  number_of_hcp_strokes
)
select
  (r.data->>'Id')::uuid as round_id,
  ps->>'PlayerName' as player_name,
  ps->>'PlayerEmoji' as player_emoji,
  ps->>'PlayerRoundStatusEmoji' as player_round_status_emoji,
  coalesce((ps->>'CourseAverageAtTheTime')::double precision, 0) as course_average_at_the_time,
  coalesce((ps->>'NumberOfHcpStrokes')::int, 0) as number_of_hcp_strokes
from mt_doc_round r
cross join lateral jsonb_array_elements(coalesce(r.data->'PlayerScores', '[]'::jsonb)) ps;

\echo 'ETL hole scores'

insert into public.hole_scores (
  "PlayerScoreId",
  strokes,
  relative_to_par,
  registered_at,
  hole_number,
  hole_par,
  hole_distance,
  hole_average,
  hole_rating
)
select
  ps_row."Id" as player_score_id,
  (hs->>'Strokes')::int as strokes,
  (hs->>'RelativeToPar')::int as relative_to_par,
  (hs->>'RegisteredAt')::timestamptz as registered_at,
  (hs->'Hole'->>'Number')::int as hole_number,
  (hs->'Hole'->>'Par')::int as hole_par,
  (hs->'Hole'->>'Distance')::int as hole_distance,
  coalesce((hs->'Hole'->>'Average')::double precision, 0) as hole_average,
  coalesce((hs->'Hole'->>'Rating')::int, 0) as hole_rating
from mt_doc_round r
cross join lateral jsonb_array_elements(coalesce(r.data->'PlayerScores', '[]'::jsonb)) ps
join public.player_scores ps_row
  on ps_row."RoundId" = (r.data->>'Id')::uuid
 and ps_row.player_name = ps->>'PlayerName'
cross join lateral jsonb_array_elements(coalesce(ps->'Scores', '[]'::jsonb)) hs;

\echo 'ETL stroke specs'

insert into public.stroke_specs (
  "HoleScoreId",
  outcome,
  put_distance
)
select
  hs_row."Id" as hole_score_id,
  (ss->>'Outcome')::int as outcome,
  nullif(ss->>'PutDistance', '')::int as put_distance
from mt_doc_round r
cross join lateral jsonb_array_elements(coalesce(r.data->'PlayerScores', '[]'::jsonb)) ps
cross join lateral jsonb_array_elements(coalesce(ps->'Scores', '[]'::jsonb)) hs
join public.player_scores ps_row
  on ps_row."RoundId" = (r.data->>'Id')::uuid
 and ps_row.player_name = ps->>'PlayerName'
join public.hole_scores hs_row
  on hs_row."PlayerScoreId" = ps_row."Id"
 and hs_row.hole_number = (hs->'Hole'->>'Number')::int
cross join lateral jsonb_array_elements(
  case
    when jsonb_typeof(hs->'StrokeSpecs') = 'array' then hs->'StrokeSpecs'
    else '[]'::jsonb
  end
) ss;

\echo 'Rounds source/target counts'
select count(*) as source_rounds from mt_doc_round;
select count(*) as target_rounds from public.rounds;
