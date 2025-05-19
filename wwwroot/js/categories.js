document.addEventListener("DOMContentLoaded", function () {

    let categoryIdToDelete = null;
    let confirmDeleteCategoryBtn = document.getElementById("confirmDeleteCategoryBtn");
    let confirmDeleteCategoryCheckbox = document.getElementById("confirmDeleteCategoryCheckbox");
    let addCategoryBtn = document.getElementById("addCategoryBtn");


    loadCategories();
    

    // Adding new category
    if (addCategoryBtn) {
        addCategoryBtn.addEventListener("click", async function () {
            let categoryName = document.getElementById("newCategory").value.trim();
            let categoryDescription = document.getElementById("newCategoryDesc").value.trim();

            if (categoryName === "" || categoryDescription === "") {
                alert("Both category name and description are required!");
                return;
            }

            try {
                let response = await fetch("/api/categories", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                        CategoryName: categoryName,
                        CategoryDescription: categoryDescription
                    })
                });

                if (!response.ok) {
                    throw new Error(await response.text());
                }

                alert("Category added successfully!");
                document.getElementById("newCategory").value = "";
                document.getElementById("newCategoryDesc").value = "";

                loadCategories(); // Refresh category list
                
            } catch (error) {
                console.error("Error adding category:", error);
                alert("Failed to add category.");
            }
        });
    } else {
        console.error("addCategoryBtn not found!");
    }





    // Delete button click event listenr 
    document.body.addEventListener("click", function (event) {
        if (event.target.classList.contains("delete-category")) {
            categoryIdToDelete = event.target.dataset.id;

            let deleteCategoryModal = bootstrap.Modal.getOrCreateInstance(document.getElementById("deleteCategoryModal"));
            deleteCategoryModal.show();
        }
    });

    // Enable delete button if checkbox checked
    confirmDeleteCategoryCheckbox.addEventListener("change", function () {
        confirmDeleteCategoryBtn.disabled = !this.checked;
    });

    // Category Deletion
    confirmDeleteCategoryBtn.addEventListener("click", async function () {
        if (!categoryIdToDelete) {
            alert("No category selected. Please select a category.");
            return;
        }

        // Checking if there are products under this category before allowing delete
        try {
            let productsResponse = await fetch(`/api/products/byCategory/${categoryIdToDelete}`);
            let products = await productsResponse.json();

            if (products.length > 0) {
                alert("Cannot delete category. There are products assigned to this category.");

                // hide modal
                let deleteModal = bootstrap.Modal.getInstance(document.getElementById("deleteCategoryModal"));
                if (deleteModal) {
                    deleteModal.hide();
                }

                // Reopen previous modal page
                let categoryModal = bootstrap.Modal.getOrCreateInstance(document.getElementById("categoryModal"));
                categoryModal.show();

                return;
            }

            // Delete category
            let response = await fetch(`/api/categories/${categoryIdToDelete}`, {
                method: "DELETE",
            });

            if (!response.ok) {
                throw new Error(await response.text());
            }

            alert("Category deleted successfully!");

            // hiding modal
            let deleteModal = bootstrap.Modal.getInstance(document.getElementById("deleteCategoryModal"));
            deleteModal.hide();

            await loadCategories();

        } catch (error) {
            alert("Failed to delete category.");
        }
    });


    // Load categories
    async function loadCategories() {
        try {
            let response = await fetch("/api/categories");
            if (!response.ok) {
                console.error("Error fetching categories");
                return;
            }

            let data = await response.json();
            let categories = data.$values ?? data;

            if (!Array.isArray(categories)) {
                console.error("API did not return an array!", categories);
                return;
            }

            console.log("Categories:", categories);

            // Populate category dropdowns
            let categoryDropdowns = document.querySelectorAll("#createProductCategory, #editProductCategory");
            categoryDropdowns.forEach(dropdown => {
                dropdown.innerHTML = '<option value="">Select Category</option>';
                categories.forEach(category => {
                    dropdown.innerHTML += `<option value="${category.id}">${category.categoryName}</option>`;
                });
            });

            // Populate category list in modal
            let categoryList = document.getElementById("categoryList");
            if (!categoryList) {
                console.error("categoryList element not found!");
                return;
            }

            // List must begin empty
            categoryList.innerHTML = "";

            categories.forEach(category => {
                let listItem = document.createElement("li");
                listItem.classList.add("list-group-item", "d-flex", "justify-content-between", "align-items-center");
                listItem.innerHTML = `
                <span>
                    <strong>${category.categoryName}</strong> - ${category.categoryDescription}
                </span>
                <div>
                    <button class="btn btn-warning btn-sm edit-category" 
                            data-id="${category.id}" 
                            data-name="${category.categoryName}" 
                            data-desc="${category.categoryDescription}" 
                            data-bs-toggle="modal" data-bs-target="#editCategoryModal">
                            Edit
                    </button>
                    <button class="btn btn-danger btn-sm delete-category" data-id="${category.id}">Delete</button>
                </div>
            `;
                categoryList.appendChild(listItem);
            });

        } catch (error) {
            console.error("Error loading categories:", error);
        }
        
    }

    // Category editing 
    document.body.addEventListener("click", function (event) {
        if (event.target.classList.contains("edit-category")) {
            console.log("Edit button clicked!");

            let categoryId = event.target.dataset.id;
            let categoryName = event.target.dataset.name;
            let categoryDesc = event.target.dataset.desc;

            document.getElementById("editCategoryId").value = categoryId;
            document.getElementById("editCategoryName").value = categoryName;
            document.getElementById("editCategoryDesc").value = categoryDesc;

            let modalElement = document.getElementById("editCategoryModal");
            if (!modalElement) {
                console.error("Modal element not found.");
                return;
            }

            let editCategoryModal = bootstrap.Modal.getOrCreateInstance(modalElement);
            editCategoryModal.show();
        }
    });


    // Category edit form submission
    document.getElementById("editCategoryForm").addEventListener("submit", async function (e) {
        e.preventDefault();

        let categoryId = document.getElementById("editCategoryId").value;
        let updatedCategory = {
            Id: parseInt(categoryId),
            CategoryName: document.getElementById("editCategoryName").value.trim(),
            CategoryDescription: document.getElementById("editCategoryDesc").value.trim()
        };

        try {
            let response = await fetch(`/api/categories/${categoryId}`, {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(updatedCategory),
            });

            if (response.ok) {
                alert("Category updated successfully!");

                // Hide the modal after
                let modalElement = document.getElementById("editCategoryModal")
                if (modalElement) {
                    let modal = bootstrap.Modal.getInstance(modalElement);
                    if (modal) {
                        modal.hide();
                    } else {
                        console.warn("Modal instance not found.");
                    }
                } else {
                    console.warn("Modal element not found.");
                }

                // refresh
                await loadCategories();
                await window.location.reload();

            } else {
                let errorMsg = await response.text()
                alert("Failed to update category.");
            }
        } catch (error) {
            console.error("Request error:", error);
            alert("Request error.");
        }
    });
});



