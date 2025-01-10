document.addEventListener("DOMContentLoaded", () => {
    const countdownElements = document.querySelectorAll('.countdown-timer');

    countdownElements.forEach(timer => {
        // Parse initial data from attributes
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
                // Stop countdown if it reaches zero
                clearInterval(interval);
                timer.textContent = "Expired";
                return;
            }

            // Update the text content
            timer.textContent = `${days} days ${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')} left`;
        };

        // Start countdown with a 1-second interval
        const interval = setInterval(updateTimer, 1000);
    });
    initializeEventListeners();

    // Ensure books are moved down if filter panel is already open
    const filterPanel = document.getElementById('filterPanel');
    const bookContainer = document.querySelector('.book-container');

    if (filterPanel.style.display === 'block') {
        bookContainer.style.marginTop = `${filterPanel.offsetHeight + 20}px`;
    }

    // Initialize price range slider
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
    // Borrow button event listeners
    document.querySelectorAll(".borrow-button").forEach(button => {
        button.removeEventListener("click", handleBorrow);
        button.addEventListener("click", handleBorrow);
    });

    // Buy button event listeners
    document.querySelectorAll(".buy-button").forEach(button => {
        button.removeEventListener("click", handleBuy);
        button.addEventListener("click", handleBuy);
    });

    // Delete button event listeners
    document.querySelectorAll(".delete-button").forEach(button => {
        button.removeEventListener("click", handleDelete);
        button.addEventListener("click", handleDelete);
    });

    // Return button event listeners
    document.querySelectorAll(".return-button").forEach(button => {
        button.removeEventListener("click", handleReturn);
        button.addEventListener("click", handleReturn);
    });

    // Join waiting list button event listeners
    document.querySelectorAll(".join-waiting-list-button").forEach(button => {
        button.removeEventListener("click", handleJoinWaitingList);
        button.addEventListener("click", handleJoinWaitingList);
    });

    // Leave waiting list button event listeners
    document.querySelectorAll(".leave-waiting-list-button").forEach(button => {
        button.removeEventListener("click", handleLeaveWaitingList);
        button.addEventListener("click", handleLeaveWaitingList);
    });

    // Release reservation button event listeners
    document.querySelectorAll(".release-reservation-button").forEach(button => {
        button.removeEventListener("click", handleReleaseReservation);
        button.addEventListener("click", handleReleaseReservation);
    });

    // Add to Cart button event listeners
    document.querySelectorAll(".add-to-cart-button").forEach(button => {
        button.removeEventListener("click", handleAddToCart);
        button.addEventListener("click", handleAddToCart);
    });

    // Add Borrow to Cart button event listeners
    document.querySelectorAll(".add-borrow-to-cart-button").forEach(button => {
        button.removeEventListener("click", handleAddBorrowToCart);
        button.addEventListener("click", handleAddBorrowToCart);
    });

    // Remove from Cart button event listeners
    document.querySelectorAll(".remove-from-cart-button").forEach(button => {
        button.removeEventListener("click", handleRemoveFromCart);
        button.addEventListener("click", handleRemoveFromCart);
    });

    // Clear Cart button event listeners
    document.querySelectorAll(".clear-cart-button").forEach(button => {
        button.removeEventListener("click", handleClearCart);
        button.addEventListener("click", handleClearCart);
    });

    // Feedback submit button(s)
    document.querySelectorAll(".submit-feedback-button").forEach(button => {
        button.removeEventListener("click", handleFeedbackSubmit);
        button.addEventListener("click", handleFeedbackSubmit);
    });

    // Star rating hovers/clicks
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

    // Filter Panel Toggle
    const filterToggle = document.getElementById("filterToggle");
    if (filterToggle) {
        filterToggle.removeEventListener("click", toggleFilter);
        filterToggle.addEventListener("click", toggleFilter);
    }

    // Apply Filters Button
    const applyFiltersButton = document.querySelector("#filterPanel .btn-success");
    if (applyFiltersButton) {
        applyFiltersButton.removeEventListener("click", applyFilters);
        applyFiltersButton.addEventListener("click", applyFilters);
    }
    // Buy Now button event listeners
    document.querySelectorAll(".buy-now-button").forEach(button => {
        button.removeEventListener("click", handleBuyNow);
        button.addEventListener("click", handleBuyNow);
    });

    // Borrow Now button event listeners
    document.querySelectorAll(".borrow-now-button").forEach(button => {
        button.removeEventListener("click", handleBorrowNow);
        button.addEventListener("click", handleBorrowNow);
    });
}
// Buy Now Handler
function handleBuyNow(event) {
    event.preventDefault();

    if (!confirm("Are you sure you want to buy this book?")) {
        return; // Exit the function if the user cancels
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
                window.location.href = data.approvalUrl; // Redirect to PayPal for payment
            } else {
                alert('An error occurred: ' + data.message);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('An error occurred while processing your request: ' + error.message);
        });
}

// Borrow Now Handler
function handleBorrowNow(event) {
    event.preventDefault();

    if (!confirm("Are you sure you want to borrow this book?")) {
        return; // Exit the function if the user cancels
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
                window.location.href = data.approvalUrl; // Redirect to PayPal for payment
            } else {
                alert('An error occurred: ' + data.message);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('An error occurred while processing your request: ' + error.message);
        });
}









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
                <button class="btn btn-primary download-button mt-2" data-book-id="${bookId}">Download</button>
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
        body: JSON.stringify({ bookId: parseInt(bookId) })
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
                <button class="btn btn-primary download-button mt-2" data-book-id="${bookId}">Download</button>
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

// Handle Add to Cart
function handleAddToCart(event) {
    event.preventDefault();
    const button = event.currentTarget;
    const bookId = button.getAttribute("data-book-id");
    const title = button.getAttribute("data-book-title"); // Ensure this attribute is set
    const price = button.getAttribute("data-book-price"); // Ensure this attribute is set

    fetch(`/ShoppingCart/Add`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "X-Requested-With": "XMLHttpRequest"
        },
        body: JSON.stringify({ BookId: parseInt(bookId), Title: title, Price: parseFloat(price), ItemType: 'Buy' }) // Set ItemType to 'Buy'
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

// Handle Add Borrow to Cart
function handleAddBorrowToCart(event) {
    event.preventDefault();
    const button = event.currentTarget;
    const bookId = button.getAttribute("data-book-id");
    const title = button.getAttribute("data-book-title"); // Ensure this attribute is set
    const price = button.getAttribute("data-book-price"); // Ensure this attribute is set

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
                    body: JSON.stringify({ BookId: parseInt(bookId), Title: title, Price: parseFloat(price), ItemType: 'Borrow' }) // Set ItemType to 'Borrow'
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



// Handle Remove from Cart
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
                location.reload(); // Reload the page to update the cart
            } else {
                alert(data.message);
            }
        })
        .catch(error => {
            console.error("Error removing from cart:", error);
            alert("An error occurred while removing from the cart.");
        });
}

// Handle Clear Cart
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

// Toggle Filter Panel and Move Books Down
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

        // Apply dark mode if enabled
        if (document.body.classList.contains('dark-mode')) {
            filterPanel.classList.add('dark-mode');
        }
    } else {
        filterPanel.style.display = 'none';
        bookContainer.style.marginTop = '0';
    }
}

// Apply Filters Function
function applyFilters() {
    const url = new URL(window.location.href);
    const category = document.getElementById('categoryDropdown').value;
    const author = document.getElementById('authorDropdown').value;
    const sort = document.getElementById('sortDropdown').value;
    const availability = document.getElementById('buyBorrowDropdown').value;
    const priceRangeMin = document.getElementById('priceRangeMin').value;
    const priceRangeMax = document.getElementById('priceRangeMax').value;
    const onSale = document.getElementById('onSaleCheckbox').checked;

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

    window.location.href = url.toString();
}

// JavaScript function for sorting books
function sortBooks(sortOption) {
    const url = new URL(window.location.href);
    if (sortOption) {
        url.searchParams.set('sort', sortOption); // Update or add the `sort` query parameter
    } else {
        url.searchParams.delete('sort'); // Remove the `sort` query parameter
    }
    window.location.href = url.toString(); // Redirect to the updated URL
}

// JavaScript function for filtering by category
function filterByCategory(category) {
    const url = new URL(window.location.href);
    url.searchParams.set('categoryFilter', decodeURIComponent(category)); // Update or add the `categoryFilter` query parameter
    url.searchParams.delete('search'); // Optional: Clear the search query when changing the category
    window.location.href = url.toString(); // Redirect to the updated URL
}

// JavaScript function for filtering by author
function filterByAuthor(author) {
    const url = new URL(window.location.href);
    url.searchParams.set('authorFilter', decodeURIComponent(author)); // Update or add the `authorFilter` query parameter
    url.searchParams.delete('search'); // Optional: Clear the search query when changing the author
    window.location.href = url.toString(); // Redirect to the updated URL
}

// JavaScript function for filtering by availability
function filterByAvailability(availability) {
    const url = new URL(window.location.href);
    url.searchParams.set('availability', decodeURIComponent(availability)); // Update or add the `availability` query parameter
    url.searchParams.delete('search'); // Optional: Clear the search query when changing the availability
    window.location.href = url.toString(); // Redirect to the updated URL
}


function handleDownload(event) {
    event.preventDefault();

    const button = event.currentTarget;
    const bookId = button.getAttribute("data-book-id");

    // Assuming the download URL is `/Books/DownloadBook`
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
            anchor.download = filename; // Set the filename
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

                // Redirect based on the referrer
                if (document.referrer.includes('/Books')) {
                    window.location.href = '/Books';
                } else if (document.referrer.includes('/Library')) {
                    window.location.href = '/Library';
                } else {
                    window.location.href = '/'; // Default redirect
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

                // Redirect based on the referrer
                if (document.referrer.includes('/Books')) {
                    window.location.href = '/Books';
                } else if (document.referrer.includes('/Library')) {
                    window.location.href = '/Library';
                } else {
                    window.location.href = '/'; // Default redirect
                }
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
                const feedbackContainer = document.querySelector(`#feedback-container-${bookId}`);
                if (feedbackContainer) {
                    // Check if the user has already posted feedback
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
                        // Update existing feedback
                        existingFeedback.innerHTML = newFeedbackHtml;
                    } else {
                        // Add new feedback
                        feedbackContainer.insertAdjacentHTML("beforeend", newFeedbackHtml);
                    }

                    // Update the average rating dynamically
                    const averageRatingContainer = document.querySelector(`#average-rating-${bookId}`);
                    if (averageRatingContainer) {
                        const newAverageRating = data.averageRating; // Assuming the server returns the new average rating
                        const ratingCount = data.ratingCount; // Assuming the server returns the new rating count
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



