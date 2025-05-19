// Ensure all DOM elements are loaded before starting
document.addEventListener("DOMContentLoaded", async function () {

    const createProductForm = document.getElementById("createProductForm");
    const editProductForm = document.getElementById("editProductForm");
    const confirmDeleteBtn = document.getElementById("confirmDeleteBtn");
    const confirmDeleteCheckbox = document.getElementById("confirmDeleteCheckbox");
    const deleteModal = document.getElementById("deleteModal");
    const filterForm = document.getElementById("filterForm");
    const lowStockBtn = document.getElementById("lowStockBtn");

    let currentUserRole = window.currentUserRole || "User";
    
    await loadProducts();
    await attachDeleteEventListeners();
    await fetchFilteredProducts();
    
    // Low stock filter button
    function updateLowStockButtonState(isActive) {
        if (isActive) {
            lowStockBtn.classList.remove("btn-outline-danger");
            lowStockBtn.classList.add("btn-danger");
            lowStockBtn.innerHTML = "Showing Low Stock";
        } else {
            lowStockBtn.classList.remove("btn-danger");
            lowStockBtn.classList.add("btn-outline-danger");
            lowStockBtn.innerHTML = "Low Stock Only";
        }
    }

    lowStockBtn.addEventListener("click", function () {
        let isLowStock = lowStockBtn.dataset.active === "true";
        lowStockBtn.dataset.active = isLowStock ? "false" : "true";
        updateLowStockButtonState(!isLowStock);
        fetchFilteredProducts();
    });

    // Reset filters
    document.getElementById("resetFilters").addEventListener("click", function () {
        filterForm.reset();
        lowStockBtn.dataset.active = "false"; // Reset low stock filter
        updateLowStockButtonState();
        fetchFilteredProducts();
    });

    // Handle form submission
    filterForm.addEventListener("submit", function (e) {
        e.preventDefault();
        fetchFilteredProducts();
    });

    function removeModalBackdrop() {
        document.body.classList.remove("modal-open");
        document.querySelectorAll(".modal-backdrop").forEach(e => e.remove());
    }

    // Reset the modal to clear dim backdrop 
    deleteModal.addEventListener("hidden.bs.modal", function () {
        confirmDeleteCheckbox.checked = false; // Reset checkbox
        confirmDeleteBtn.disabled = true;
        removeModalBackdrop(); // Disable button again
    });
    
    // Handle Create Product Form Submission
    if (createProductForm) {
        createProductForm.addEventListener("submit", async function (e) {
            e.preventDefault();

            let newProduct = 
            {
                Name: document.getElementById("createProductName").value,
                Description: document.getElementById("createProductDesc").value,
                CategoryId: parseInt(document.getElementById("createProductCategory").value),
                Price: parseFloat(document.getElementById("createProductPrice").value),
                ProductStockAmount: parseInt(document.getElementById("createProductStock").value),
                LowStockThreshold: parseInt(document.getElementById("createProductLowStock").value)
            };

            try {
                let response = await fetch("/api/products", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(newProduct),
                });

                if (response.ok) {
                    alert("Product created successfully!");
                    await fetchFilteredProducts(); // re-renders table with updated data
                    bootstrap.Modal.getInstance(document.getElementById("createProductModal")).hide(); // hide modal
                } else {
                    console.error("Error creating product:", await response.json());
                    alert("Failed to create product.");
                }
            } catch (error) {
                console.error("Request error:", error);
                alert("Something went wrong.");
            }
        });
    } else {
        console.error("createProductForm not found!");
    }


    // Edit product event listener
    document.body.addEventListener("click", function (event) {
        if (event.target.classList.contains("edit-product")) {
            
            let productId = event.target.dataset.id;

            if (!productId) {
                console.error("Product ID is missing!");
                return;
            }
            
            console.log("Clicked Edit Button - Product ID:", productId);
            console.log("   Data Attributes Received:");
            console.log("   Name:", event.target.dataset.name);
            console.log("   Description:", event.target.dataset.desc);
            console.log("   Category:", event.target.dataset.category);
            console.log("   Price:", event.target.dataset.price);
            console.log("   Stock:", event.target.dataset.stock);
            console.log("   LowStock:", event.target.dataset.lowstock);

            console.log(` Starting to edit Product ID: ${productId}`);
            document.getElementById("editProductId").value = productId;
            document.getElementById("editProductName").value = event.target.dataset.name;
            document.getElementById("editProductDesc").value = event.target.dataset.desc;


            let categoryDropdown = document.getElementById("editProductCategory");
            let selectedCategory = event.target.dataset.category;
            if (categoryDropdown && selectedCategory) {
                categoryDropdown.value = selectedCategory;
            }

            document.getElementById("editProductPrice").value = event.target.dataset.price;
            document.getElementById("editProductStock").value = event.target.dataset.stock;
            document.getElementById("editProductLowStock").value = event.target.dataset.lowstock;
        }
    });

    // Edit product form submission
    if (editProductForm) {
        editProductForm.addEventListener("submit", async function (e) {
            e.preventDefault();

            let productId = document.getElementById("editProductId").value.trim();

            if (!productId) {
                console.error("No Product ID found.");
                alert("Error: Product ID is missing.");
                return;
            }

            let updatedProduct = 
            {
                Id: parseInt(productId, 10),
                Name: document.getElementById("editProductName").value,
                Description: document.getElementById("editProductDesc").value,
                CategoryId: parseInt(document.getElementById("editProductCategory").value),
                Price: parseFloat(document.getElementById("editProductPrice").value),
                ProductStockAmount: parseInt(document.getElementById("editProductStock").value),
                LowStockThreshold: parseInt(document.getElementById("editProductLowStock").value)
            };

            console.log("Sending Update Request:", updatedProduct);

            try {
                let response = await fetch(`/api/products/${productId}`, {
                    method: "PUT",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(updatedProduct),
                });

                if (response.ok) {
                    alert("Product updated successfully!");
                    location.reload();
                } else {
                    console.error("Error updating product:", await response.json());
                    alert("Failed to update product.");
                }
            } catch (error) {
                console.error("Request error:", error);
                alert("Request Error.");
            }
        });
    } else {
        console.error("editProductForm not found!");
    }

    if (confirmDeleteBtn) {
        confirmDeleteBtn.disabled = true;
    }

    // Enable delete button after checkbox
    if (confirmDeleteCheckbox) {
        confirmDeleteCheckbox.addEventListener("change", function () {
            confirmDeleteBtn.disabled = !this.checked;
        });
    } else {
        console.error("confirmDeleteCheckbox not found");
    }

    // Delete confirm button handler
    if (confirmDeleteBtn) {
        confirmDeleteBtn.addEventListener("click", async function () {
            if (!confirmDeleteCheckbox.checked) {
                alert("Please confirm deletion before proceeding.");
                return;
            }

            const productId = this.getAttribute("data-id");

            try {
                let response = await fetch(`/api/products/${productId}`, {
                    method: "DELETE"
                });

                if (response.ok) {
                    alert("Product deleted successfully!");
                    const modal = bootstrap.Modal.getInstance(deleteModal);
                    modal.hide();
                    removeModalBackdrop();
                    await fetchFilteredProducts();
                } else {
                    const errorText = await response.text();
                    showDeleteError(errorText || "Could not delete the product.");
                }
            } catch (error) {
                console.error("Request error:", error);
                showDeleteError("Something went wrong while deleting the product.");
            }
        });

    } else {
        console.error("confirmDeleteBtn not found!");
    }
    
    
    // Reattach event listeners to delete button. - Workaround for delete button not registering clicks correctly
    function attachDeleteEventListeners() {
        document.body.addEventListener("click", function (event) {
            if (event.target.classList.contains("delete-product")) {
                let productId = event.target.dataset.id;
                document.getElementById("confirmDeleteBtn").setAttribute("data-id", productId);
                new bootstrap.Modal(document.getElementById("deleteModal")).show();
            }
        });
    }
    
    // Function for updating table with fetched and filtered products
    function renderFilteredProductTable(products) {
        const productTableBody = document.getElementById("productTableBody");
        productTableBody.innerHTML = "";

        if (products.length === 0) {
            productTableBody.innerHTML = `<tr><td colspan="6" class="text-center">No products found.</td></tr>`;
            return;
        }

        products.forEach(product => {
            const stockStatus = product.productStockAmount < product.lowStockThreshold
                ? `<span class="badge bg-danger">Low Stock</span>`
                : `<span class="badge bg-success">Stock OK</span>`;

            const row = `
                <tr>
                    <td>${product.name}</td>
                    <td>${product.category ? product.category.categoryName : "Uncategorized"}</td>
                    <td>$${product.price.toFixed(2)}</td>
                    <td>${product.productStockAmount}</td>
                    <td>${stockStatus}</td>
                    ${currentUserRole === "Admin" ? ` 
                        <td>
                            ${currentUserRole === "Admin" ? `
                                <button class="btn btn-warning btn-sm edit-product" 
                                    data-id="${product.id}" 
                                    data-name="${product.name}" 
                                    data-desc="${product.description}" 
                                    data-category="${product.categoryId}" 
                                    data-price="${product.price}" 
                                    data-stock="${product.productStockAmount}" 
                                    data-lowstock="${product.lowStockThreshold}" 
                                    data-bs-toggle="modal" 
                                    data-bs-target="#editModal">
                                    Edit
                                </button>
                                <button class="btn btn-danger btn-sm delete-product" 
                                    data-id="${product.id}" 
                                    data-bs-toggle="modal" 
                                    data-bs-target="#deleteModal">
                                    Delete
                                </button>
                            ` : ''}
                        </td>
                    ` : ''}
                </tr>
            `;
            productTableBody.innerHTML += row;
        });
    }

    // Async
    // Fetch products with filters 
    async function fetchFilteredProducts() {
        const loadingSym = document.getElementById("loading");
        loadingSym.style.display = "block";
        
        const search = document.getElementById("search").value.trim();
        const categoryId = document.getElementById("categoryId").value;
        const minPrice = document.getElementById("minPrice").value;
        const maxPrice = document.getElementById("maxPrice").value;
        const sortBy = document.getElementById("sortBy").value;
        const lowStockOnly = lowStockBtn.dataset.active === "true";

        const queryParams = new URLSearchParams();
        if (search)
            queryParams.append("search", search);
        if (categoryId)
            queryParams.append("categoryId", categoryId);
        if (minPrice)
            queryParams.append("minPrice", minPrice);
        if (maxPrice)
            queryParams.append("maxPrice", maxPrice);
        if (sortBy)
            queryParams.append("sortBy", sortBy);
        if (lowStockOnly)
            queryParams.append("lowStockOnly", "true");

        try {
            const response = await fetch(`/api/products?${queryParams.toString()}`);
            const data = await response.json();
            renderFilteredProductTable(data);
            
        } catch (error) {
            console.error("Error fetching filtered products:", error);
        }
        finally {
            loadingSym.style.display = "none";
        }
    }
    
    
    
    // Initial load for all products from db. -- A bit redundant now that fetchFiltered was created; but breaks if I remove it. 
    async function loadProducts() {
        try {
            let response = await fetch("/api/products");
            let data = await response.json();

            if (!Array.isArray(data)) {
                console.error("API did not return an array:", data);
                return;
            }

            console.log("Response:", data);

            let tableBody = document.getElementById("productTableBody");
            if (!tableBody) {
                console.error("Table body not found!");
                return;
            }

            // Table must start empty
            tableBody.innerHTML = "";

            // Loop and populate table
            data.forEach(product => {
                let row = `<tr>
                <td>${product.name}</td>
                <td>${product.category ? product.category.categoryName : "Uncategorized"}</td>
                <td>$${product.price.toFixed(2)}</td>
                <td>${product.productStockAmount}</td>
                <td>
                    ${product.productStockAmount < product.lowStockThreshold ?
                    '<span style="color: red;">Low Stock</span>' :
                    '<span style="color: green;">OK</span>'}
                </td>
                ${currentUserRole === "Admin" ? `
                    <td>
                    ${currentUserRole === "Admin" ? `
                            <button class="btn btn-warning btn-sm edit-product" 
                                data-id="${product.id}" 
                                data-name="${product.name}" 
                                data-desc="${product.description}" 
                                data-category="${product.categoryId}" 
                                data-price="${product.price}" 
                                data-stock="${product.productStockAmount}" 
                                data-lowstock="${product.lowStockThreshold}" 
                                data-bs-toggle="modal" 
                                data-bs-target="#editModal">
                                Edit
                            </button>
                            <button class="btn btn-danger btn-sm delete-product" 
                                data-id="${product.id}" 
                                data-bs-toggle="modal" 
                                data-bs-target="#deleteModal">
                                Delete
                            </button>
                    ` : ''}
                    </td>
                ` : ''}
            </tr>`;
                tableBody.innerHTML += row;
            });

        } catch (error) {
            console.error("Error loading products to table:", error);
        }
    }
    
    // Showing delete error for products that are part of active orders
    function showDeleteError(message) {
        const modalBody = document.querySelector("#deleteModal .modal-body");

        let alertBox = document.getElementById("deleteErrorMsg");
        if (!alertBox) {
            alertBox = document.createElement("div");
            alertBox.className = "alert alert-danger mt-3";
            alertBox.id = "deleteErrorMsg";
            modalBody.appendChild(alertBox);
        }

        alertBox.textContent = message;
    }

    document.getElementById("deleteModal").addEventListener("hidden.bs.modal", () => {
        const alertBox = document.getElementById("deleteErrorMsg");
        if (alertBox) alertBox.remove();
    });

});



















