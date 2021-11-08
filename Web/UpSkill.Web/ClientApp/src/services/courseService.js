import axios from "axios"; 
import { Base_URL } from '../utils/baseUrlConstant';

const API_URL = Base_URL + "Courses/";

const token = localStorage.getItem("token");

const numberCoursesToShow = 5;

const initialCourses = [
  {
    id: 1,
    courseName: 'Marketing',
    coachName: 'Jim Wilber',
    imageName: 'Marketing.png',
  },
  {
    id: 2,
    courseName: 'Design',
    coachName: 'Tom Smith',
    imageName: 'Design.png',
  },
  {
    id: 3,
    courseName: 'Management',
    coachName: 'Sarah Coleman',
    imageName: 'Management.png',
  },
  {
    id: 4,
    courseName: 'HTML&CSS',
    coachName: 'David Can',
    imageName: 'HTML&CSS.png',
  },
  {
    id: 5,
    courseName: 'Java',
    coachName: 'Emily Hill',
    imageName: 'Java.png',
  },
];

export const getCourses = async (currentPage) => {
  //      let res = await request(``, 'Get');
  let arr = [];
  arr.push(
    ...initialCourses.slice(
      0,
      currentPage * numberCoursesToShow + numberCoursesToShow
    )
  );

  return arr;
};

export const courseDetailsContent = async (course) => {
  const response = await axios.get(`${API_URL}contentDetails?id=${course.id}`, { headers: { "Authorization": `Bearer ${token}` } }, {
    course
  });
  return response;
};
