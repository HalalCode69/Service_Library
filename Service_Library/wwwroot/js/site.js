document.addEventListener("DOMContentLoaded", () => {
    initializeEventListeners();
    const countdownElements = document.querySelectorAll('.countdown-timer');

    countdownElements.forEach(timer => {
        let days = parseInt(timer.getAttribute('data-days'));
        let hours = parseInt(timer.getAttribute('data-hours'));
        let minutes = parseInt(timer.getAttribute('data-minutes'));
        let seconds = parseInt(timer.getAttribute('data-seconds'));

        const updateTimer = () => {
            if (seconds > 0) {
                seconds--;
            } else if (minutes > 0) {
                seconds = 59;
                minutes--;
            } else if (hours > 0) {
                seconds = 59;
                minutes = 59;
                hours--;
            } else if (days > 0) {
                seconds = 59;
                minutes = 59;
                hours = 23;
                days--;
            } else {
                clearInterval(interval);
                timer.textContent = "Expired";
                return;
            }

            timer.textContent = `${days} days ${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')} left`;
        };

        const interval = setInterval(updateTimer, 1000);
    });

    const filterPanel = document.getElementById('filterPanel');
    const bookContainer = document.querySelector('.book-container');

    if (filterPanel.style.display === 'block') {
        bookContainer.style.marginTop = `${filterPanel.offsetHeight + 20}px`;
    }

    var priceRangeSlider = document.getElementById('priceRangeSlider');
    var minPrice = parseFloat(document.getElementById('priceRangeMin').getAttribute('data-min-price'));
    var maxPrice = parseFloat(document.getElementById('priceRangeMax').getAttribute('data-max-price'));
    var startMin = parseFloat(document.getElementById('priceRangeMin').value) || minPrice;
    var startMax = parseFloat(document.getElementById('priceRangeMax').value) || maxPrice;

    noUiSlider.create(priceRangeSlider, {
        start: [startMin, startMax],
        connect: true,
        range: {
            'min': minPrice,
            'max': maxPrice
        },
        tooltips: [true, true],
        format: {
            to: function (value) {
                return value.toFixed(2);
            },
            from: function (value) {
                return Number(value);
            }
        }
    });

    priceRangeSlider.noUiSlider.on('update', function (values, handle) {
        document.getElementById('priceRangeMin').value = values[0];
        document.getElementById('priceRangeMax').value = values[1];
        document.getElementById('priceRangeMinLabel').innerText = '$' + values[0];
        document.getElementById('priceRangeMaxLabel').innerText = '$' + values[1];
    });
});

function initializeEventListeners() {
    document.querySelectorAll(".borrow-button").forEach(button => {
        button.removeEventListener("click", handleBorrow);
        button.addEventListener("click", handleBorrow);
    });

    document.querySelectorAll(".buy-button").forEach(button => {
        button.removeEventListener("click", handleBuy);
        button.addEventListener("click", handleBuy);
    });

    document.querySelectorAll(".delete-button").forEach(button => {
        button.removeEventListener("click", handleDelete);
        button.addEventListener("click", handleDelete);
    });

    document.querySelectorAll(".return-button").forEach(button => {
        button.removeEventListener("click", handleReturn);
        button.addEventListener("click", handleReturn);
    });

    document.querySelectorAll(".join-waiting-list-button").forEach(button => {
        button.removeEventListener("click", handleJoinWaitingList);
        button.addEventListener("click", handleJoinWaitingList);
    });

    document.querySelectorAll(".leave-waiting-list-button").forEach(button => {
        button.removeEventListener("click", handleLeaveWaitingList);
        button.addEventListener("click", handleLeaveWaitingList);
    });


    document.querySelectorAll(".release-reservation-button").forEach(button => {
        button.removeEventListener("click", handleReleaseReservation);
        button.addEventListener("click", handleReleaseReservation);
    });

    document.querySelectorAll(".add-to-cart-button").forEach(button => {
        button.removeEventListener("click", handleAddToCart);
        button.addEventListener("click", handleAddToCart);
    });

    document.querySelectorAll(".add-borrow-to-cart-button").forEach(button => {
        button.removeEventListener("click", handleAddBorrowToCart);
        button.addEventListener("click", handleAddBorrowToCart);
    });

    document.querySelectorAll(".remove-from-cart-button").forEach(button => {
        button.removeEventListener("click", handleRemoveFromCart);
        button.addEventListener("click", handleRemoveFromCart);
    });

    document.querySelectorAll(".clear-cart-button").forEach(button => {
        button.removeEventListener("click", handleClearCart);
        button.addEventListener("click", handleClearCart);
    });

    document.querySelectorAll(".submit-feedback-button").forEach(button => {
        button.removeEventListener("click", handleFeedbackSubmit);
        button.addEventListener("click", handleFeedbackSubmit);
    });

    document.querySelectorAll(".star-rating").forEach(ratingDiv => {
        const stars = ratingDiv.querySelectorAll(".star");
        stars.forEach(star => {
            star.removeEventListener("mouseover", handleStarMouseOver);
            star.addEventListener("mouseover", handleStarMouseOver);

            star.removeEventListener("click", handleStarClick);
            star.addEventListener("click", handleStarClick);
        });

        ratingDiv.removeEventListener("mouseout", resetStarHighlight);
        ratingDiv.addEventListener("mouseout", resetStarHighlight);
    });

    const filterToggle = document.getElementById("filterToggle");
    if (filterToggle) {
        filterToggle.removeEventListener("click", toggleFilter);
        filterToggle.addEventListener("click", toggleFilter);
    }

    const applyFiltersButton = document.querySelector("#filterPanel .btn-success");
    if (applyFiltersButton) {
        applyFiltersButton.removeEventListener("click", applyFilters);
        applyFiltersButton.addEventListener("click", applyFilters);
    }
    document.querySelectorAll(".buy-now-button").forEach(button => {
        button.removeEventListener("click", handleBuyNow);
        button.addEventListener("click", handleBuyNow);
    });

    document.querySelectorAll(".borrow-now-button").forEach(button => {
        button.removeEventListener("click", handleBorrowNow);
        button.addEventListener("click", handleBorrowNow);
    });
}
function handleBuyNow(event) {
    event.preventDefault();

    if (!confirm("Are you sure you want to buy this book?")) {
        return;
    }

    const button = event.currentTarget;
    const bookId = button.getAttribute("data-book-id");
    const bookTitle = button.getAttribute("data-book-title");
    const bookPrice = button.getAttribute("data-book-price");

    fetch(`/api/Payment/BuyOrBorrowItem`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "X-Requested-With": "XMLHttpRequest"
        },
        body: JSON.stringify({ bookId: parseInt(bookId), itemType: 'Buy', title: bookTitle, price: parseFloat(bookPrice) })
    })
        .then(response => {
            if (!response.ok) {
                return response.text().then(text => { throw new Error(text || `HTTP error! Status: ${response.status}`); });
            }
            return response.json();
        })
        .then(data => {
            if (data.approvalUrl) {
                window.location.href = data.approvalUrl;
            } else {
                alert('An error occurred: ' + data.message);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('An error occurred while processing your request: ' + error.message);
        });
}

function handleBorrowNow(event) {
    event.preventDefault();

    if (!confirm("Are you sure you want to borrow this book?")) {
        return;
    }

    const button = event.currentTarget;
    const bookId = button.getAttribute("data-book-id");
    const bookTitle = button.getAttribute("data-book-title");
    const bookPrice = button.getAttribute("data-book-price");

    fetch(`/api/Payment/BuyOrBorrowItem`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "X-Requested-With": "XMLHttpRequest"
        },
        body: JSON.stringify({ bookId: parseInt(bookId), itemType: 'Borrow', title: bookTitle, price: parseFloat(bookPrice) })
    })
        .then(response => {
            if (!response.ok) {
                return response.text().then(text => { throw new Error(text || `HTTP error! Status: ${response.status}`); });
            }
            return response.json();
        })
        .then(data => {
            if (data.approvalUrl) {
                window.location.href = data.approvalUrl;
            } else {
                alert('An error occurred: ' + data.message);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('An error occurred while processing your request: ' + error.message);
        });
}


function handleAddToCart(event) {
    event.preventDefault();
    const button = event.currentTarget;
    const bookId = button.getAttribute("data-book-id");
    const title = button.getAttribute("data-book-title");
    const price = button.getAttribute("data-book-price");

    fetch(`/ShoppingCart/Add`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "X-Requested-With": "XMLHttpRequest"
        },
        body: JSON.stringify({ BookId: parseInt(bookId), Title: title, Price: parseFloat(price), ItemType: 'Buy' })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert(data.message);
            } else {
                alert(data.message);
            }
        })
        .catch(error => {
            console.error("Error adding to cart:", error);
            alert("An error occurred while adding to the cart.");
        });
}

function handleAddBorrowToCart(event) {
    event.preventDefault();
    const button = event.currentTarget;
    const bookId = button.getAttribute("data-book-id");
    const title = button.getAttribute("data-book-title");
    const price = button.getAttribute("data-borrow-price");

    fetch(`/ShoppingCart/GetBorrowCount`, {
        method: "GET",
        headers: {
            "Content-Type": "application/json",
            "X-Requested-With": "XMLHttpRequest"
        }
    })
        .then(response => response.json())
        .then(data => {
            const totalBorrowed = data.borrowedCount + data.cartBorrowCount;
            if (totalBorrowed >= 3) {
                alert("You can only borrow up to 3 books at the same time.");
            } else {
                fetch(`/ShoppingCart/Add`, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "X-Requested-With": "XMLHttpRequest"
                    },
                    body: JSON.stringify({ BookId: parseInt(bookId), Title: title, Price: parseFloat(price), ItemType: 'Borrow' })
                })
                    .then(response => response.json())
                    .then(data => {
                        if (data.success) {
                            alert(data.message);
                        } else {
                            alert(data.message);
                        }
                    })
                    .catch(error => {
                        console.error("Error adding to cart:", error);
                        alert("An error occurred while adding to the cart.");
                    });
            }
        })
        .catch(error => {
            console.error("Error checking borrow count:", error);
            alert("An error occurred while checking the borrow count.");
        });
}


function handleRemoveFromCart(event) {
    event.preventDefault();
    const button = event.currentTarget;
    const bookId = button.getAttribute("data-book-id");

    fetch(`/ShoppingCart/Remove`, {
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
                location.reload();
            } else {
                alert(data.message);
            }
        })
        .catch(error => {
            console.error("Error removing from cart:", error);
            alert("An error occurred while removing from the cart.");
        });
}
function handleClearCart(event) {
    event.preventDefault();

    fetch(`/ShoppingCart/Clear`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "X-Requested-With": "XMLHttpRequest"
        }
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert(data.message);
            } else {
                alert(data.message);
            }
        })
        .catch(error => {
            console.error("Error clearing cart:", error);
            alert("An error occurred while clearing the cart.");
        });
}
function toggleFilter() {
    const filterPanel = document.getElementById('filterPanel');
    const bookContainer = document.querySelector('.book-container');

    if (!filterPanel) {
        console.error("Filter panel not found!");
        return;
    }

    console.log("Toggling filter panel...");
    if (filterPanel.style.display === 'none' || !filterPanel.style.display) {
        filterPanel.style.display = 'block';
        bookContainer.style.marginTop = `${filterPanel.offsetHeight + 20}px`;

        if (document.body.classList.contains('dark-mode')) {
            filterPanel.classList.add('dark-mode');
        }
    } else {
        filterPanel.style.display = 'none';
        bookContainer.style.marginTop = '0';
    }
}
function applyFilters() {
    const url = new URL(window.location.href);
    const category = document.getElementById('categoryDropdown').value;
    const author = document.getElementById('authorDropdown').value;
    const sort = document.getElementById('sortDropdown').value;
    const availability = document.getElementById('buyBorrowDropdown').value;
    const priceRangeMin = document.getElementById('priceRangeMin').value;
    const priceRangeMax = document.getElementById('priceRangeMax').value;
    const onSale = document.getElementById('onSaleCheckbox').checked;
    const genres = Array.from(document.getElementById('genreDropdown').selectedOptions).map(option => option.value);

    if (category) url.searchParams.set('categoryFilter', category);
    else url.searchParams.delete('categoryFilter');

    if (author) url.searchParams.set('authorFilter', author);
    else url.searchParams.delete('authorFilter');

    if (sort) url.searchParams.set('sort', sort);
    else url.searchParams.delete('sort');

    if (availability) url.searchParams.set('availability', availability);
    else url.searchParams.delete('availability');

    if (priceRangeMin) url.searchParams.set('priceRangeMin', priceRangeMin);
    else url.searchParams.delete('priceRangeMin');

    if (priceRangeMax) url.searchParams.set('priceRangeMax', priceRangeMax);
    else url.searchParams.delete('priceRangeMax');

    if (onSale) url.searchParams.set('onSale', 'true');
    else url.searchParams.delete('onSale');

    if (genres.length > 0) url.searchParams.set('genreFilter', genres.join(','));
    else url.searchParams.delete('genreFilter');

    window.location.href = url.toString();
}

function sortBooks(sortOption) {
    const url = new URL(window.location.href);
    if (sortOption) {
        url.searchParams.set('sort', sortOption);
    } else {
        url.searchParams.delete('sort');
    }
    window.location.href = url.toString();
}

function filterByCategory(category) {
    const url = new URL(window.location.href);
    url.searchParams.set('categoryFilter', decodeURIComponent(category));
    url.searchParams.delete('search');
    window.location.href = url.toString();
}

function filterByAuthor(author) {
    const url = new URL(window.location.href);
    url.searchParams.set('authorFilter', decodeURIComponent(author));
    url.searchParams.delete('search');
    window.location.href = url.toString();
}

function filterByAvailability(availability) {
    const url = new URL(window.location.href);
    url.searchParams.set('availability', decodeURIComponent(availability));
    url.searchParams.delete('search');
    window.location.href = url.toString();
}

function handleDownload(event) {
    event.preventDefault();

    const button = event.currentTarget;
    const bookId = button.getAttribute("data-book-id");
    const downloadUrl = `/Books/DownloadBook?bookId=${bookId}`;

    fetch(downloadUrl, {
        method: "POST",
        headers: {
            "Content-Type": "application/x-www-form-urlencoded",
            "X-Requested-With": "XMLHttpRequest"
        }
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            const contentDisposition = response.headers.get("Content-Disposition");
            let filename = "downloaded_file";
            if (contentDisposition && contentDisposition.indexOf("attachment") !== -1) {
                const matches = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/.exec(contentDisposition);
                if (matches != null && matches[1]) {
                    filename = matches[1].replace(/['"]/g, '');
                }
            }
            return response.blob().then(blob => ({ blob, filename }));
        })
        .then(({ blob, filename }) => {
            const url = window.URL.createObjectURL(blob);
            const anchor = document.createElement("a");
            anchor.href = url;
            anchor.download = filename;
            document.body.appendChild(anchor);
            anchor.click();
            document.body.removeChild(anchor);
            window.URL.revokeObjectURL(url);
        })
        .catch(error => {
            console.error("Error downloading book:", error);
            alert("An error occurred while downloading the book.");
        });
}
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
            body: JSON.stringify(bookId)
        })
            .then(response => {
                if (!response.ok) throw new Error(`HTTP error! Status: ${response.status}`);
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    alert(data.message);
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

document.addEventListener("DOMContentLoaded", () => {
    initializeEventListeners();
    document.querySelectorAll(".delete-button").forEach(button => {
        button.removeEventListener("click", handleDeleteBook);
        button.addEventListener("click", handleDeleteBook);
    });
});
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
                if (document.referrer.includes('/Books')) {
                    window.location.href = '/Books';
                } else if (document.referrer.includes('/Books/Library')) {
                    window.location.href = '/Books/Library';
                } else {
                    window.location.href = '/';
                }
            } else {
                alert(data.message);
            }
        })
        .catch(error => {
            console.error("Error:", error);
            alert("An error occurred while processing your request: " + error.message);
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
            body: JSON.stringify({ transactionId })
        })
            .then(response => {
                if (!response.ok) throw new Error(`HTTP error! Status: ${response.status}`);
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    alert(data.message);
                    location.reload();
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
                const waitingListElement = document.querySelector(`#waiting-list-count-${bookId}`);
                const userWaitingPositionElement = document.querySelector(`#user-waiting-position-${bookId}`);
                if (waitingListElement) {
                    waitingListElement.style.display = 'none';
                }
                if (userWaitingPositionElement) {
                    userWaitingPositionElement.innerHTML = `You are in position: ${data.userPosition} in the waiting list.`;
                    userWaitingPositionElement.classList.replace("text-info", "text-warning");
                    userWaitingPositionElement.style.display = 'block';
                } else {
                    const newUserWaitingPositionElement = document.createElement('p');
                    newUserWaitingPositionElement.id = `user-waiting-position-${bookId}`;
                    newUserWaitingPositionElement.className = 'text-warning';
                    newUserWaitingPositionElement.innerHTML = `You are in position: ${data.userPosition} in the waiting list.`;
                    waitingListElement.insertAdjacentElement('afterend', newUserWaitingPositionElement);
                }
                button.innerText = "Leave Waiting List";
                button.classList.replace("join-waiting-list-button", "leave-waiting-list-button");
                initializeEventListeners();
            } else {
                alert(data.message);
            }
        })
        .catch(error => {
            console.error("Error joining waiting list:", error);
            alert("An error occurred while joining the waiting list.");
        });
}

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
                const waitingListElement = document.querySelector(`#waiting-list-count-${bookId}`);
                const userWaitingPositionElement = document.querySelector(`#user-waiting-position-${bookId}`);
                if (waitingListElement) {
                    waitingListElement.innerHTML = `Users in Waitlist for borrow: ${data.waitingListCount} users`;
                    waitingListElement.classList.replace("text-warning", "text-info");
                    waitingListElement.style.display = 'block';
                }
                if (userWaitingPositionElement) {
                    userWaitingPositionElement.style.display = 'none';
                }
                button.innerText = "Join Waiting List";
                button.classList.replace("leave-waiting-list-button", "join-waiting-list-button");
                initializeEventListeners();
            } else {
                alert(data.message);
            }
        })
        .catch(error => {
            console.error("Error leaving waiting list:", error);
            alert("An error occurred while leaving the waiting list.");
        });
}

function handleReleaseReservation(event) {
    event.preventDefault();

    const button = event.currentTarget;
    const bookId = button.getAttribute("data-book-id");

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
                if (document.referrer.includes('/Books')) {
                    window.location.href = '/Books';
                } else if (document.referrer.includes('/Books/Library')) {
                    window.location.href = '/Books/Library';
                } else {
                    window.location.href = '/';
                }
            } else {
                alert(data.message);
            }
        })
        .catch(error => {
            console.error("Error releasing reservation:", error);
            alert("An error occurred while releasing the reservation.");
        })
        .finally(() => {
            button.disabled = false;
        });
}
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
    }, 1000);
}

function handleStarMouseOver(event) {
    const currentStar = event.currentTarget;
    const ratingValue = parseInt(currentStar.getAttribute("data-value"));
    const starsContainer = currentStar.closest(".star-rating");
    const stars = starsContainer.querySelectorAll(".star");
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
    starsContainer.setAttribute("data-selected-rating", ratingValue);
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

    const commentField = document.getElementById(`feedback-comment-${bookId}`);
    const commentText = commentField ? commentField.value.trim() : "";

    const requestData = {
        bookId: parseInt(bookId),
        rating: ratingVal,
        comment: commentText
    };

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
                const feedbackContainer = document.querySelector(`#feedback-container-${bookId}`);
                if (feedbackContainer) {
                    const existingFeedback = feedbackContainer.querySelector('.existing-feedback[data-user-id="current-user"]');
                    const newFeedbackHtml = `
                        <div class="existing-feedback mb-2" data-user-id="current-user">
                            <div class="display-rating">
                                ${Array.from({ length: 5 }, (_, i) => `<span style="color:${i < ratingVal ? "gold" : "gray"};">&#9733;</span>`).join('')}
                            </div>
                            <p class="mt-1 mb-0"><em>${commentText}</em></p>
                            <small class="text-muted">You on ${new Date().toLocaleDateString()}</small>
                            <hr />
                        </div>
                    `;

                    if (existingFeedback) {
                        existingFeedback.innerHTML = newFeedbackHtml;
                    } else {
                        feedbackContainer.insertAdjacentHTML("beforeend", newFeedbackHtml);
                    }

                    const averageRatingContainer = document.querySelector(`#average-rating-${bookId}`);
                    if (averageRatingContainer) {
                        const newAverageRating = data.averageRating;
                        const ratingCount = data.ratingCount;
                        averageRatingContainer.innerHTML = `
                            <strong>Average Rating:</strong>
                            ${Array.from({ length: 5 }, (_, i) => `<span style="color:${i < Math.round(newAverageRating) ? "gold" : "gray"};">&#9733;</span>`).join('')}
                            <small>(${ratingCount} ratings)</small>
                        `;
                    }
                } else {
                    console.error("Feedback container not found.");
                }
            } else {
                alert(data.message);
            }
        })
        .catch(error => {
            console.error("Error:", error);
            alert("An error occurred while processing your request.");
        });
}



