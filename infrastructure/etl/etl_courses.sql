\echo 'ETL courses'
SET search_path TO disclive_development, public;

insert into public.courses (
  id,
  name,
  created_at,
  layout,
  country,
  admins,
  latitude,
  longitude
)
select
  (c.data->>'Id')::uuid as id,
  c.data->>'Name' as name,
  (c.data->>'CreatedAt')::timestamptz as created_at,
  c.data->>'Layout' as layout,
  c.data->>'Country' as country,
  coalesce(array(select jsonb_array_elements_text(c.data->'Admins')), '{}') as admins,
  (c.data->'Coordinates'->>'Latitude')::numeric as latitude,
  (c.data->'Coordinates'->>'Longitude')::numeric as longitude
from mt_doc_course c;

\echo 'ETL course holes'

insert into public.course_holes (
  "CourseId",
  number,
  par,
  distance,
  average,
  rating
)
select
  (c.data->>'Id')::uuid as course_id,
  (h->>'Number')::int as number,
  (h->>'Par')::int as par,
  (h->>'Distance')::int as distance,
  coalesce((h->>'Average')::double precision, 0) as average,
  coalesce((h->>'Rating')::int, 0) as rating
from mt_doc_course c
cross join lateral jsonb_array_elements(coalesce(c.data->'Holes', '[]'::jsonb)) h;

\echo 'Courses source/target counts'
select count(*) as source_courses from mt_doc_course;
select count(*) as target_courses from public.courses;
