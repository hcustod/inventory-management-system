document.addEventListener("DOMContentLoaded", function () {
    document.getElementById("checkoutBtn").disabled = true;

    document.getElementById("orderForm").addEventListener("submit", function (e) {
        e.preventDefault();
    });
});

// Get and insert products into create order 
async function fetchProducts() {
    try {
        const response = await fetch("/api/products");
        if (!response.ok) {
            throw new Error(`HTTP error! Status: ${response.status}`);
        }

        const data = await response.json();
        console.log("Response:", data);

        if (!Array.isArray(data)) {
            console.error("Did not return an array:", data);
            return;
        }

        let productList = document.getElementById("productList");
        if (!productList) {
            console.error("product list element not found");
            return;
        }

        productList.innerHTML = "";

        data.forEach(product => {
            if (product.productStockAmount > 0) { 
                let item = document.createElement("tr");
                item.innerHTML = `
                    <td>${product.name}</td>
                    <td class="product-price" data-id="${product.id}">$${product.price.toFixed(2)}</td>
                    <td>${product.productStockAmount}</td>
                    <td>
                        <button type="button" class="decrement btn btn-sm btn-outline-secondary" data-id="${product.id}">-</button>
                        <input type="number" class="product-quantity text-center" data-id="${product.id}" min="0" max="${product.productStockAmount}" value="0" style="width: 50px;" readonly>
                        <button type="button" class="increment btn btn-sm btn-outline-secondary" data-id="${product.id}">+</button>
                    </td>
                `;
                productList.appendChild(item);
            }
        });

        console.log("Products added to the modal.");
    } catch (error) {
        console.error("Error fetching products:", error);
    }
}



// Event delegation on counters - fixes buttons freezing
document.getElementById("productList").addEventListener("click", function (event) {
    let target = event.target;
    let input;

    if (target.classList.contains("increment")) {
        input = target.previousElementSibling;
        let max = parseInt(input.max);
        if (parseInt(input.value) < max) {
            input.value = parseInt(input.value) + 1;
        }
    } else if (target.classList.contains("decrement")) {
        input = target.nextElementSibling;
        if (parseInt(input.value) > 0) {
            input.value = parseInt(input.value) - 1;
        }
    }

    if (input) {
        updateCheckoutButton();
        updateTotalPrice();
    }
});

// Enable checkout only if items are selected
function updateCheckoutButton() {
    let hasItems = [...document.querySelectorAll(".product-quantity")].some(input => parseInt(input.value) > 0);
    document.getElementById("checkoutBtn").disabled = !hasItems;
}

// Update total price
function updateTotalPrice() {
    let totalPrice = 0;

    document.querySelectorAll(".product-quantity").forEach(input => {
        const quantity = parseInt(input.value);
        if (quantity > 0) {
            let priceElement = document.querySelector(`.product-price[data-id="${input.dataset.id}"]`);
            let price = parseFloat(priceElement.textContent.replace("$", ""));
            totalPrice += price * quantity;
        }
    });

    let totalDisplay = document.getElementById("orderTotal");
    if (totalDisplay) {
        totalDisplay.textContent = totalPrice.toFixed(2);
    }
}

// Call fetchProducts when modal opens
document.getElementById("orderModal").addEventListener("show.bs.modal", async function () {
    await fetchProducts();
});

// --- Delete order feature ---
let selectedOrderId = null;

document.querySelectorAll(".delete-order-btn").forEach(button => {
    button.addEventListener("click", function () {
        selectedOrderId = this.getAttribute("data-id");
        document.getElementById("confirmDeleteBtn").disabled = true;
        document.getElementById("confirmDeleteCheckbox").checked = false;
    });
});

document.getElementById("confirmDeleteCheckbox").addEventListener("change", function () {
    document.getElementById("confirmDeleteBtn").disabled = !this.checked;
});

document.getElementById("confirmDeleteBtn").addEventListener("click", async function () {
    if (!selectedOrderId) return;

    try {
        const response = await fetch(`/api/orders/${selectedOrderId}`, { method: "DELETE" });

        if (response.ok) {
            alert("Order deleted successfully!");
            window.location.reload();
        } else {
            const errorData = await response.json();
            alert(`Failed to delete order: ${errorData.message}`);
        }
    } catch (error) {
        console.error("Error deleting order:", error);
        alert("Failed to delete order.");
    }
});


// --- Order create modal features --- 
document.getElementById("checkoutBtn").addEventListener("click", function (e) {
    e.preventDefault();

    const selectedProducts = [];
    document.querySelectorAll(".product-quantity").forEach(input => {
        const productId = input.dataset.id;
        const quantity = parseInt(input.value);
        if (quantity > 0) {
            selectedProducts.push({ productId, quantity });
        }
    });

    if (selectedProducts.length === 0) {
        alert("Please select at least one product.");
        return;
    }

    let summaryHtml = "";
    let totalPrice = 0;

    selectedProducts.forEach(op => {
        let productElement = document.querySelector(`.product-quantity[data-id="${op.productId}"]`);
        let productName = productElement.closest("tr").querySelector("td:first-child").textContent;
        let productPrice = parseFloat(productElement.closest("tr").querySelector("td:nth-child(2)").textContent.replace("$", ""));
        let itemTotal = productPrice * op.quantity;
        totalPrice += itemTotal;

        summaryHtml += `
            <tr>
                <td>${productName}</td>
                <td class="text-center">${op.quantity}</td>
                <td class="text-end">$${itemTotal.toFixed(2)}</td>
            </tr>
        `;
    });

    document.getElementById("orderSummary").innerHTML = summaryHtml;
    document.getElementById("orderTotal").textContent = `${totalPrice.toFixed(2)}`;

    // Enable button
    document.getElementById("confirmOrderBtn").disabled = false;

    // Open checkout modal
    let checkoutModal = new bootstrap.Modal(document.getElementById("checkoutModal"));
    checkoutModal.show();
});

document.getElementById("confirmOrderBtn").addEventListener("click", async function () {
    // Show loading symbol
    const loader = document.getElementById("orderLoader");
    loader.style.display = "block";

    const userName = document.getElementById("guestName")?.value.trim();
    const userEmail = document.getElementById("guestEmail")?.value.trim();
    const products = [];

    document.querySelectorAll(".product-quantity").forEach(input => {
        const productId = input.getAttribute("data-id");
        const quantity = parseInt(input.value);

        if (productId && !isNaN(quantity) && quantity > 0) {
            products.push({
                ProductId: parseInt(productId, 10),
                Quantity: quantity
            });
        }
    });

    if (!userName || !userEmail) {
        alert("Please enter your name and email.");
        loader.style.display = "none";
        return;
    }

    if (products.length === 0) {
        alert("Please select at least one product.");
        loader.style.display = "none";
        return;
    }

    const order = {
        userName,
        userEmail,
        OrderProducts: products
    };
    
    try {
        const response = await fetch("/api/orders", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(order)
        });

        loader.style.display = "none";  // Hide spinner after response

        if (response.ok) {
            alert("Order placed successfully!");

            let checkoutModal = bootstrap.Modal.getInstance(document.getElementById("checkoutModal"));
            if (checkoutModal) {
                checkoutModal.hide();
            }

            window.location.reload(); // Refresh the page to update order list
        } else {
            const errorData = await response.json();
            console.error("Order submission failed:", errorData);
            alert(`Failed to place order: ${errorData.message}`);
        }
    } catch (error) {
        console.error("Error submitting order:", error);
        alert("Failed to place order.");
    }
});


