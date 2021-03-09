import React, { useState, useEffect } from "react";
import { connect, ConnectedProps } from "react-redux";
import colors from "../../../colors";
import { ApplicationState } from "../../../store";
import {
  actionCreators as coursesActionCreator,
  Course,
} from "../../../store/Courses";
import { Hole } from "../../../store/Rounds";
import { useMountEffect } from "../../../utils";
import "./CreateRound.css";

const mapState = (state: ApplicationState) => {
  return {
    courses: state.courses?.courses,
    friends: state.user?.userDetails?.friends,
    username: state.user?.user?.username || "",
  };
};

const connector = connect(mapState, {
  ...coursesActionCreator,
});

type PropsFromRedux = ConnectedProps<typeof connector>;

type Props = PropsFromRedux & {
  setSelectedLayout: React.Dispatch<React.SetStateAction<Course | undefined>>;
  selectedLayout: Course | undefined;
};

interface Chunk {
  header: JSX.Element;
  par: JSX.Element;
}

const tableComps = (holeNumber: number, holePar: number) => {
  const header = (key: number) => (
    <th key={key} className="has-text-centered">
      {holeNumber} <br />
    </th>
  );
  const par = (key: number) => (
    <td key={key} className="has-text-centered">
      <i>{holePar}</i>
    </td>
  );
  return { header, par };
};

const chunkArray = (holes: Hole[], chunk_size: number) => {
  var index = 0;
  var arrayLength = holes.length;
  var tempArray = [];

  for (index = 0; index < arrayLength; index += chunk_size) {
    const myChunk = holes.slice(index, index + chunk_size);
    const size = myChunk.length;
    myChunk.length = chunk_size;
    const tableChunk = myChunk
      .fill({} as Hole, size, chunk_size)
      .map((c) => tableComps(c.number, c.par));

    tempArray.push(tableChunk);
  }

  return tempArray;
};

const CourseSelector = (props: Props) => {
  const { fetchCourses, setSelectedLayout, selectedLayout } = props;
  const [courseFilter, setCourseFilter] = useState("");
  const [selectedCourse, setSelectedCourse] = useState<string>();
  const [availableLayouts, setAvailableLayouts] = useState<
    Course[] | undefined
  >(undefined);
  const courseSelected = (courseName: string) => {
    if (!props.courses) return;
    setSelectedCourse(courseName);
    const layouts = props.courses.find((c) => c[0] === courseName);
    if (!layouts || layouts[1].length === 0) return;
    console.log(layouts[1]);
    layouts && setAvailableLayouts(layouts[1]);
    layouts && setSelectedLayout(layouts[1][0]);
  };
  const layoutSelected = (courseId: string) => {
    const layout =
      availableLayouts && availableLayouts.find((l) => l.id === courseId);
    layout && setSelectedLayout(layout);
  };

  useMountEffect(() => {
    fetchCourses(courseFilter);
  });

  useEffect(() => {
    courseFilter.length > 2 && fetchCourses(courseFilter);
  }, [courseFilter, fetchCourses]);

  //   var chunks = chunkArray(currentCourse.holes, 6, setEditHole);

  return (
    <div>
      {selectedCourse ? (
        <span
          onClick={() => {
            setSelectedCourse(undefined);
            setAvailableLayouts(undefined);
          }}
          className="tag is-large mb-2"
        >
          {selectedCourse}
          <button className="delete ml-3"></button>
        </span>
      ) : (
        <>
          <div className="field">
            <div className="control has-icons-left">
              <input
                className="input"
                type="text"
                placeholder="Search"
                onChange={(e) => {
                  setCourseFilter(e.target.value);
                }}
                style={{ backgroundColor: colors.field }}
              />
              <span className="icon is-left">
                <i className="fas fa-search" aria-hidden="true"></i>
              </span>
            </div>
          </div>
          <div className="panel">
            {props.courses?.slice(0, 5).map((c) => (
              <span
                onClick={() => courseSelected(c[0])}
                key={c[0]}
                className={`panel-block ${
                  selectedCourse === c[0] && "is-active"
                }`}
              >
                <span className="panel-icon">
                  <i className="fas fa-cloud-sun" aria-hidden="true"></i>
                </span>
                {c[0]}
              </span>
            ))}
          </div>
        </>
      )}
      {availableLayouts && availableLayouts.length > 0 && (
        <div>
          {availableLayouts.map((l) => (
            <div
              className="box py-1 mb-2 px-3"
              key={l.id}
              style={{
                backgroundColor: colors.background,
                border: selectedLayout?.id === l.id ? "3px solid black" : "",
              }}
              onClick={() => layoutSelected(l.id)}
            >
              <h2 className="subtitle is-6 mb-1">{l.layout}</h2>
              {chunkArray(l.holes, 9).map((c, i) => {
                return (
                  <table
                    key={i}
                    className="table is-narrower is-bordered py-0 my-1 is-fullwidth"
                    style={{ backgroundColor: colors.table }}
                  >
                    <thead>
                      <tr>
                        <th>Hole</th>
                        {c.map((t, i) => t.header(i))}
                      </tr>
                    </thead>
                    <tbody>
                      <tr>
                        <th>Par</th>
                        {c.map((t, i) => t.par(i))}
                      </tr>
                    </tbody>
                  </table>
                );
              })}
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default connector(CourseSelector);
