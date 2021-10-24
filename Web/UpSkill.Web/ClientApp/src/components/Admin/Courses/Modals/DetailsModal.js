import React, { useState, useEffect } from "react";
import UserProfilePic from "../../../../assets/userProfilePic.png";
import GoogleLogo from "../../../../assets/img/courses/Image 39.png";
import "./DetailsModal.css";

function DetailsModal({ closeModal }) {
  
  const [title, setTitle] = useState("");
  const [coachName, setCoachName] = useState("");
  const [description, setDescription] = useState("");
  const [price, setPrice] = useState(0);
  const [category, setCategory] = useState("");

    

  useEffect(() => {        
    setPrice(localStorage.getItem("Price"));
    setDescription(localStorage.getItem("Description"));
    setTitle(localStorage.getItem("Title"));
    setCategory(localStorage.getItem("CategoryId"));
    setCoachName(localStorage.getItem("FullName"));    
  }, []);


  return (
    <div className="detailsModal-background">
      <div className="detailsModal-container">
        <div className="detailsModal-header">
          <div className="titleCloseBtn">
            <button className="the-x-btn" onClick={() => closeModal(false)}>
              X
            </button>
          </div>
          <div className="header-els-container">
            <div className="detailsModal-title">
              <h1>{title}</h1>
            </div>
            <div className="row detailsModal-coach-info">
              <div className="col-2 detailsModal-img-coach-wrapper">
                <img
                  src={UserProfilePic}
                  alt="User"
                  className="img-fluid rounded detailsModal-img-coach"
                ></img>
              </div>
              <div className="col-2 detailsModal-coach-name-wrapper">
                <span>Created by</span>
                <h3>{coachName}</h3>
                <h6>
                  <img src={GoogleLogo}></img>
                </h6>
              </div>
            </div>
          </div>
        </div>
        <div className="detailsModal-body">
          <h2>Course Description</h2>
          <p>{description}</p>
        </div>
        <div className="detailsModal-image-course-wrapper">
          <div className="detailsModel-image-course"></div>
          <div className="detailsModel-img-course-body">
            <h4>What you'll learn</h4>
            <p>
              - Learn more information about Digital Marketing - Improve your
              time management - Solve problems
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
export default DetailsModal;
