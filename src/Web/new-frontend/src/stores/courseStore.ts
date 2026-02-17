import { create } from "zustand";
import type { CourseVm } from "../types";
import { api } from "../api/client";

interface CourseState {
  courses: CourseVm[];
  isLoading: boolean;
  error: string | null;

  fetchCourses: (filter?: string) => Promise<void>;
  createCourse: (cmd: {
    courseName: string;
    layoutName: string;
    latitude: number;
    longitude: number;
    numberOfHoles: number;
    par4s: number[];
    par5s: number[];
  }) => Promise<CourseVm>;
  clear: () => void;
}

function groupCoursesByName(courses: CourseVm[]): Map<string, CourseVm[]> {
  const map = new Map<string, CourseVm[]>();
  for (const course of courses) {
    const existing = map.get(course.name);
    if (existing) {
      existing.push(course);
    } else {
      map.set(course.name, [course]);
    }
  }
  return map;
}

export { groupCoursesByName };

export const useCourseStore = create<CourseState>((set, get) => ({
  courses: [],
  isLoading: false,
  error: null,

  fetchCourses: async (filter = "") => {
    set({ isLoading: true, error: null });
    try {
      const courses = await api.getCourses(filter);
      set({ courses, isLoading: false });
    } catch {
      set({ error: "Failed to load courses", isLoading: false });
    }
  },

  createCourse: async (cmd) => {
    const newCourse = await api.createCourse(cmd);
    set({ courses: [newCourse, ...get().courses] });
    return newCourse;
  },

  clear: () => set({ courses: [], isLoading: false, error: null }),
}));
