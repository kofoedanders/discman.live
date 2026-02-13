\echo 'ETL hall of fame'
SET search_path TO disclive_development, public;

insert into public.hall_of_fames (
  id,
  hall_of_fame_type,
  updated_at,
  month,
  year,
  created_at,
  most_birdies_count,
  most_birdies_per_round,
  most_birdies_username,
  most_birdies_time_of_entry,
  most_birdies_new_this_month,
  most_bogies_count,
  most_bogies_per_round,
  most_bogies_username,
  most_bogies_time_of_entry,
  most_bogies_new_this_month,
  most_rounds_count,
  most_rounds_username,
  most_rounds_time_of_entry,
  most_rounds_new_this_month,
  best_round_average_round_average,
  best_round_average_username,
  best_round_average_time_of_entry,
  best_round_average_new_this_month
)
select
  (h.data->>'Id')::uuid as id,
  case when h.mt_dotnet_type like '%MonthHallOfFame%' then 'month' else 'base' end as hall_of_fame_type,
  coalesce((h.data->>'UpdatedAt')::timestamptz, '0001-01-01 00:00:00+00'::timestamptz) as updated_at,
  coalesce((h.data->>'Month')::int, 0) as month,
  coalesce((h.data->>'Year')::int, 0) as year,
  coalesce((h.data->>'CreatedAt')::timestamptz, '0001-01-01 00:00:00+00'::timestamptz) as created_at,
  (h.data->'MostBirdies'->>'Count')::int as most_birdies_count,
  (h.data->'MostBirdies'->>'PerRound')::double precision as most_birdies_per_round,
  h.data->'MostBirdies'->>'Username' as most_birdies_username,
  (h.data->'MostBirdies'->>'TimeOfEntry')::timestamptz as most_birdies_time_of_entry,
  coalesce((h.data->'MostBirdies'->>'NewThisMonth')::boolean, false) as most_birdies_new_this_month,
  (h.data->'MostBogies'->>'Count')::int as most_bogies_count,
  (h.data->'MostBogies'->>'PerRound')::double precision as most_bogies_per_round,
  h.data->'MostBogies'->>'Username' as most_bogies_username,
  (h.data->'MostBogies'->>'TimeOfEntry')::timestamptz as most_bogies_time_of_entry,
  coalesce((h.data->'MostBogies'->>'NewThisMonth')::boolean, false) as most_bogies_new_this_month,
  (h.data->'MostRounds'->>'Count')::int as most_rounds_count,
  h.data->'MostRounds'->>'Username' as most_rounds_username,
  (h.data->'MostRounds'->>'TimeOfEntry')::timestamptz as most_rounds_time_of_entry,
  coalesce((h.data->'MostRounds'->>'NewThisMonth')::boolean, false) as most_rounds_new_this_month,
  (h.data->'BestRoundAverage'->>'RoundAverage')::double precision as best_round_average_round_average,
  h.data->'BestRoundAverage'->>'Username' as best_round_average_username,
  (h.data->'BestRoundAverage'->>'TimeOfEntry')::timestamptz as best_round_average_time_of_entry,
  coalesce((h.data->'BestRoundAverage'->>'NewThisMonth')::boolean, false) as best_round_average_new_this_month
from mt_doc_halloffame h;

\echo 'Hall of fame source/target counts'
select count(*) as source_hof from mt_doc_halloffame;
select count(*) as target_hof from public.hall_of_fames;
