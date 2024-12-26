document.addEventListener("DOMContentLoaded", () => {
    initializeEventListeners();
});

function initializeEventListeners() {
    // Borrow button event listeners
    document.querySelectorAll(".borrow-button").forEach(button => {
        button.removeEventListener("click", handleBorrow); // Remove previous event listener
        button.addEventListener("click", handleBorrow); // Add new event listener
    });

    // Buy button event listeners
    document.querySelectorAll(".buy-button").forEach(button => {
        button.removeEventListener("click", handleBuy); // Remove previous event listener
        button.addEventListener("click", handleBuy); // Add new event listener
    });

    // Delete button event listeners
    document.querySelectorAll(".delete-button").forEach(button => {
        button.removeEventListener("click", handleDelete); // Remove previous event listener
        button.addEventListener("click", handleDelete); // Add new event listener
    });

    // Return button event listeners
    document.querySelectorAll(".return-button").forEach(button => {
        button.removeEventListener("click", handleReturn); // Remove previous event listener
        button.addEventListener("click", handleReturn); // Add new event listener
    });

    // Join waiting list button event listeners
    document.querySelectorAll(".join-waiting-list-button").forEach(button => {
        button.removeEventListener("click", handleJoinWaitingList); // Remove previous event listener
        button.addEventListener("click", handleJoinWaitingList); // Add new event listener
    });

    // Leave waiting list button event listeners
    document.querySelectorAll(".leave-waiting-list-button").forEach(button => {
        button.removeEventListener("click", handleLeaveWaitingList); // Remove previous event listener
        button.addEventListener("click", handleLeaveWaitingList); // Add new event listener
    });

    // Release reservation button event listeners
    document.querySelectorAll(".release-reservation-button").forEach(button => {
        button.removeEventListener("click", handleReleaseReservation); // Remove old listeners
        button.addEventListener("click", handleReleaseReservation);   // Add new listener
    });

    // Feedback submit button(s)
    document.querySelectorAll(".submit-feedback-button").forEach(button => {
        button.removeEventListener("click", handleFeedbackSubmit);
        button.addEventListener("click", handleFeedbackSubmit);
    });

    // Star rating hovers/clicks
    document.querySelectorAll(".star-rating").forEach(ratingDiv => {
        const stars = ratingDiv.querySelectorAll(".star");
        // Remove old listeners, then add new
        stars.forEach(star => {
            star.removeEventListener("mouseover", handleStarMouseOver);
            star.removeEventListener("click", handleStarClick);

            star.addEventListener("mouseover", handleStarMouseOver);
            star.addEventListener("click", handleStarClick);
        });

        // Reset highlight on mouseout
        ratingDiv.removeEventListener("mouseout", resetStarHighlight);
        ratingDiv.addEventListener("mouseout", resetStarHighlight);
    });
}



// Borrow Book Handler
// Borrow Book Handler
function handleBorrow(event) {
    event.preventDefault();

    if (!confirm("Are you sure you want to borrow this book?")) {
        return; // Exit the function if the user cancels
    }

    const button = event.currentTarget;
    const bookId = button.getAttribute("data-book-id");
    button.disabled = true; // Prevent multiple clicks

    fetch(`/Books/BorrowBook`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "X-Requested-With": "XMLHttpRequest"
        },
        body: JSON.stringify({ bookId })
    })
        .then(response => {
            if (!response.ok) {
                if (response.status === 401) {
                    throw new Error("User not logged in");
                }
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            if (data.success) {
                alert(data.message);

                // Update the button container dynamically
                const buttonContainer = document.querySelector(`#button-container-${bookId}`);
                const returnTimestamp = new Date(data.returnTimestamp);

                buttonContainer.innerHTML = `
                    <p class="text-success"><strong>Borrowed:</strong> <span id="remaining-time-${bookId}"></span></p>
                    <button class="btn btn-danger return-button" data-transaction-id="${data.transactionId}" data-book-id="${bookId}">Return Book</button>
                    <button class="btn btn-primary buy-button mt-2" data-book-id="${bookId}">Buy Now</button>
                    <div class="your-feedback mt-3">
                        <h6>Your Feedback</h6>
                        <div class="star-rating" data-book-id="${bookId}" data-selected-rating="0">
                            <span class="star" data-value="1">&#9733;</span>
                            <span class="star" data-value="2">&#9733;</span>
                            <span class="star" data-value="3">&#9733;</span>
                            <span class="star" data-value="4">&#9733;</span>
                            <span class="star" data-value="5">&#9733;</span>
                        </div>
                        <textarea id="feedback-comment-${bookId}" class="form-control mt-2" rows="3" placeholder="Leave your comment here..."></textarea>
                        <button type="button" class="btn btn-info mt-2 submit-feedback-button" data-book-id="${bookId}">Submit Feedback</button>
                    </div>
                `;

                // Start the countdown timer
                startCountdown(`remaining-time-${bookId}`, returnTimestamp);

                // Reinitialize event listeners for new buttons and feedback
                initializeEventListeners();
            } else {
                alert(data.message);
            }
        })
        .catch(error => {
            console.error("Error:", error);
            if (error.message === "User not logged in") {
                alert("Please log in to borrow this book.");
            } else {
                alert("An error occurred while processing your request.");
            }
        })
        .finally(() => {
            button.disabled = false; // Re-enable the button
        });
}

// Buy Book Handler
function handleBuy(event) {
    event.preventDefault();

    if (!confirm("Are you sure you want to buy this book?")) {
        return; // Exit the function if the user cancels
    }

    const button = event.currentTarget;
    const bookId = button.getAttribute("data-book-id");

    fetch(`/Books/BuyBook`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "X-Requested-With": "XMLHttpRequest"
        },
        body: JSON.stringify({ bookId })
    })
        .then(response => {
            if (!response.ok) {
                if (response.status === 401) {
                    throw new Error("User not logged in");
                }
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            if (data.success) {
                alert(data.message);

                const buttonContainer = document.querySelector(`#button-container-${bookId}`);
                buttonContainer.innerHTML = `
                    <p class="text-success"><strong>You own this book!</strong></p>
                    <div class="your-feedback mt-3">
                        <h6>Your Feedback</h6>
                        <div class="star-rating" data-book-id="${bookId}" data-selected-rating="0">
                            <span class="star" data-value="1">&#9733;</span>
                            <span class="star" data-value="2">&#9733;</span>
                            <span class="star" data-value="3">&#9733;</span>
                            <span class="star" data-value="4">&#9733;</span>
                            <span class="star" data-value="5">&#9733;</span>
                        </div>
                        <textarea id="feedback-comment-${bookId}" class="form-control mt-2" rows="3" placeholder="Leave your comment here..."></textarea>
                        <button type="button" class="btn btn-info mt-2 submit-feedback-button" data-book-id="${bookId}">Submit Feedback</button>
                    </div>
                `;

                // Reinitialize event listeners for new buttons and feedback
                initializeEventListeners();
            } else {
                alert(data.message);
            }
        })
        .catch(error => {
            console.error("Error:", error);
            if (error.message === "User not logged in") {
                alert("Please log in to buy this book.");
            } else {
                alert("An error occurred while processing your request.");
            }
        });
}






// Delete Book Handler
function handleDelete(event) {
    event.preventDefault();

    const button = event.currentTarget;
    const bookId = button.getAttribute("data-book-id");

    if (confirm("Are you sure you want to delete this book from your library?")) {
        fetch(`/Books/DeleteFromLibrary`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "X-Requested-With": "XMLHttpRequest"
            },
            body: JSON.stringify(bookId) // Send bookId as a raw integer
        })
            .then(response => {
                if (!response.ok) throw new Error(`HTTP error! Status: ${response.status}`);
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    alert(data.message);

                    // Remove the book card from the UI
                    const bookCard = document.querySelector(`[data-book-id='${bookId}']`);
                    if (bookCard) {
                        bookCard.remove();
                    }
                } else {
                    alert(data.message || "Failed to delete the book.");
                }
            })
            .catch(error => {
                console.error("Error deleting book:", error);
                alert("An error occurred while deleting the book.");
            });
    }
}

// Add Delete Book Handler to Initialization
document.addEventListener("DOMContentLoaded", () => {
    initializeEventListeners();

    // Delete button event listeners
    document.querySelectorAll(".delete-button").forEach(button => {
        button.removeEventListener("click", handleDeleteBook); // Remove previous event listener
        button.addEventListener("click", handleDeleteBook); // Add new event listener
    });
});

// Return Book Handler
function handleReturn(event) {
    event.preventDefault();

    const button = event.currentTarget;
    const transactionId = button.getAttribute("data-transaction-id");
    const bookId = button.getAttribute("data-book-id");

    fetch(`/Books/ReturnBook`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "X-Requested-With": "XMLHttpRequest"
        },
        body: JSON.stringify({ transactionId })
    })
        .then(response => {
            if (!response.ok) throw new Error(`HTTP error! Status: ${response.status}`);
            return response.json();
        })
        .then(data => {
            if (data.success) {
                alert(data.message);

                const buttonContainer = document.querySelector(`#button-container-${bookId}`);
                buttonContainer.innerHTML = `
                <button class="btn btn-success borrow-button" data-book-id="${bookId}">Borrow</button>
                <button class="btn btn-primary buy-button mt-2" data-book-id="${bookId}">Buy Now</button>
                `;

                initializeEventListeners();
            } else {
                alert(data.message);
            }
        })
        .catch(error => {
            console.error("Error:", error);
            alert("An error occurred while processing your request.");
        });
}

function confirmReturnBook(transactionId, bookId) {
    if (confirm("Are you sure you want to return this book?")) {
        fetch(`/Books/ReturnBook`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "X-Requested-With": "XMLHttpRequest"
            },
            body: JSON.stringify({ transactionId }) // Send the correct transaction ID
        })
            .then(response => {
                if (!response.ok) throw new Error(`HTTP error! Status: ${response.status}`);
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    alert(data.message);
                    location.reload(); // Reload the page to refresh the library
                } else {
                    alert(data.message);
                }
            })
            .catch(error => {
                console.error("Error:", error);
                alert("An error occurred while returning the book.");
            });
    }
}

// Join Waiting List Handler
function handleJoinWaitingList(event) {
    event.preventDefault();
    const button = event.currentTarget;
    const bookId = button.getAttribute("data-book-id");

    fetch(`/Books/JoinWaitingList`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "X-Requested-With": "XMLHttpRequest"
        },
        body: JSON.stringify({ bookId: parseInt(bookId) })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Update UI dynamically
                const waitingListElement = document.querySelector(`#waiting-list-count-${bookId}`);
                if (waitingListElement) {
                    if (data.userPosition) {
                        waitingListElement.textContent = `You are in position ${data.userPosition} in the waiting list.`;
                    } else {
                        waitingListElement.textContent = `Users in Waitlist for borrow: ${data.waitingListCount}`;
                    }
                }

                // Update button to "Leave Waiting List"
                button.innerText = "Leave Waiting List";
                button.classList.replace("join-waiting-list-button", "leave-waiting-list-button");
                initializeEventListeners();
            } else {
                // Show only meaningful messages
                alert(data.message);
            }
        })
        .catch(error => {
            console.error("Error joining waiting list:", error);
            alert("An error occurred while joining the waiting list.");
        });
}

// Leave Waiting List Handler
function handleLeaveWaitingList(event) {
    event.preventDefault();

    const button = event.currentTarget;
    const bookId = button.getAttribute("data-book-id");

    fetch(`/Books/LeaveWaitingList`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "X-Requested-With": "XMLHttpRequest"
        },
        body: JSON.stringify({ bookId: parseInt(bookId) })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Update UI dynamically
                const waitingListElement = document.querySelector(`#waiting-list-count-${bookId}`);
                if (waitingListElement) {
                    waitingListElement.textContent = `Users in Waitlist for borrow: ${data.waitingListCount}`;
                }

                // Update button to "Join Waiting List"
                button.innerText = "Join Waiting List";
                button.classList.replace("leave-waiting-list-button", "join-waiting-list-button");
                initializeEventListeners();
            } else {
                // Only display one error message
                alert(data.message);
            }
        })
        .catch(error => {
            console.error("Error leaving waiting list:", error);
            alert("An error occurred while leaving the waiting list.");
        });
}

// Handle Release Reservation
function handleReleaseReservation(event) {
    event.preventDefault();

    const button = event.currentTarget;
    const bookId = button.getAttribute("data-book-id");

    // Disable the button to prevent multiple clicks
    button.disabled = true;

    fetch(`/Books/LeaveWaitingList`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "X-Requested-With": "XMLHttpRequest"
        },
        body: JSON.stringify({ bookId: parseInt(bookId) })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert(data.message);

                const buttonContainer = document.querySelector(`#button-container-${bookId}`);
                if (buttonContainer) {
                    if (data.isReserved) {
                        // If reserved for another user
                        buttonContainer.innerHTML = `
                            <p class="text-danger"><strong>Reserved:</strong> This book is currently reserved for another user.</p>
                        `;
                    } else if (data.waitingListCount > 0) {
                        // If waiting list exists
                        buttonContainer.innerHTML = `
                            <p class="text-info">Users in Waitlist for borrow: ${data.waitingListCount}</p>
                            <button class="btn btn-warning btn-sm join-waiting-list-button" data-book-id="${bookId}">
                                Join Waiting List
                            </button>
                        `;
                    } else {
                        // If no reservation or waiting list, show "Borrow" and "Buy Now" buttons
                        buttonContainer.innerHTML = `
                            <button class="btn btn-success borrow-button" data-book-id="${bookId}">Borrow</button>
                            <button class="btn btn-primary buy-button mt-2" data-book-id="${bookId}">Buy Now</button>
                        `;
                    }
                }

                initializeEventListeners(); // Reinitialize listeners for new buttons
            } else {
                alert(data.message); // Show error message from the server
            }
        })
        .catch(error => {
            console.error("Error releasing reservation:", error);
            alert("An error occurred while releasing the reservation.");
        })
        .finally(() => {
            button.disabled = false; // Re-enable button
        });
}

// Countdown Function
function startCountdown(elementId, returnTimestamp) {
    const intervalId = setInterval(() => {
        const now = new Date();
        const remainingTime = new Date(returnTimestamp) - now;

        if (remainingTime <= 0) {
            document.getElementById(elementId).textContent = "Overdue";
            clearInterval(intervalId);
            return;
        }

        const days = Math.floor(remainingTime / (1000 * 60 * 60 * 24));
        const hours = Math.floor((remainingTime % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
        const minutes = Math.floor((remainingTime % (1000 * 60)) / (1000 * 60));

        document.getElementById(elementId).textContent = `${days} days, ${hours} hours, ${minutes} mins left`;
    }, 1000); // Update every second
}

// --- NEW: Star Rating & Feedback Submission ---

function handleStarMouseOver(event) {
    const currentStar = event.currentTarget;
    const ratingValue = parseInt(currentStar.getAttribute("data-value"));
    const starsContainer = currentStar.closest(".star-rating");
    const stars = starsContainer.querySelectorAll(".star");

    // Highlight all stars up to the hovered one
    stars.forEach(star => {
        const starVal = parseInt(star.getAttribute("data-value"));
        if (starVal <= ratingValue) {
            star.classList.add("hovered");
        } else {
            star.classList.remove("hovered");
        }
    });
}

function handleStarClick(event) {
    const currentStar = event.currentTarget;
    const ratingValue = parseInt(currentStar.getAttribute("data-value"));
    const starsContainer = currentStar.closest(".star-rating");
    const stars = starsContainer.querySelectorAll(".star");

    // Store the selected rating in a data attribute
    starsContainer.setAttribute("data-selected-rating", ratingValue);

    // Update the visual appearance
    stars.forEach(star => {
        const starVal = parseInt(star.getAttribute("data-value"));
        if (starVal <= ratingValue) {
            star.classList.add("selected");
        } else {
            star.classList.remove("selected");
        }
    });
}

function resetStarHighlight(event) {
    // Revert to the currently selected rating
    const starsContainer = event.currentTarget;
    const selectedRating = parseInt(starsContainer.getAttribute("data-selected-rating") || "0");
    const stars = starsContainer.querySelectorAll(".star");

    stars.forEach(star => {
        star.classList.remove("hovered");
        const starVal = parseInt(star.getAttribute("data-value"));
        if (starVal <= selectedRating) {
            star.classList.add("selected");
        } else {
            star.classList.remove("selected");
        }
    });
}

function handleFeedbackSubmit(event) {
    event.preventDefault();

    const button = event.currentTarget;
    const bookId = button.getAttribute("data-book-id");

    // Find the star rating container
    const ratingContainer = document.querySelector(`.star-rating[data-book-id="${bookId}"]`);
    if (!ratingContainer) {
        alert("Rating container not found.");
        return;
    }

    const ratingVal = parseInt(ratingContainer.getAttribute("data-selected-rating") || "0");
    if (ratingVal < 1 || ratingVal > 5) {
        alert("Please choose a rating between 1 and 5 stars.");
        return;
    }

    // Find the comment textarea
    const commentField = document.getElementById(`feedback-comment-${bookId}`);
    const commentText = commentField ? commentField.value.trim() : "";

    // Build the request data
    const requestData = {
        bookId: parseInt(bookId),
        rating: ratingVal,
        comment: commentText
    };

    // Send to server
    fetch("/Books/SubmitFeedback", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "X-Requested-With": "XMLHttpRequest"
        },
        body: JSON.stringify(requestData)
    })
        .then(response => response.json())
        .then(data => {
        if (data.success) {
            alert(data.message);
            // Example approach: re-fetch the updated feedback or just do an inline update
            const feedbackContainer = document.querySelector("#feedback-container-" + bookId);
            const newFeedbackHtml = `
                <div class="existing-feedback mb-2">
                    <div class="display-rating">
                        <!-- etc. star loop from data.rating -->
                    </div>
                    <p class="mt-1 mb-0"><em>${commentText}</em></p>
                    <small class="text-muted">You on ${new Date().toLocaleDateString()}</small>
                    <hr />
                </div>
            `;
            feedbackContainer.insertAdjacentHTML("beforeend", newFeedbackHtml);
        }
        else {
                alert(data.message);
                // Optionally reload the page or dynamically refresh the feedback list
            }
        })
        .catch(error => {
            console.error("Error:", error);
            alert("An error occurred while processing your request.");
        });
}
