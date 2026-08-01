let allCategories = [];
let allDepartments = [];

document.addEventListener(
    "DOMContentLoaded",
    async function () {
        document
            .getElementById("categorySearch")
            .addEventListener(
                "input",
                applyCategoryFilters
            );

        document
            .getElementById("categoryDepartmentFilter")
            .addEventListener(
                "change",
                applyCategoryFilters
            );

        await loadDepartments();
        await loadCategories();
    }
);

/*
 * Loads all departments for the filter and modal dropdowns.
 */
async function loadDepartments() {
    try {
        const response =
            await fetch("/Panel/GetDepartments");

        if (response.status === 401) {
            window.location.href =
                "/Account/Login";

            return;
        }

        if (!response.ok) {
            throw new Error(
                "Departmanlar alınamadı."
            );
        }

        allDepartments =
            await response.json();

        renderDepartmentFilter();
        renderCategoryDepartmentOptions();
    }
    catch (error) {
        showCategoryPageMessage(
            "danger",
            error.message ??
            "Departmanlar yüklenirken hata oluştu."
        );
    }
}

/*
 * Loads all request categories from PanelController.
 */
async function loadCategories() {
    const loadingArea =
        document.getElementById(
            "categoryLoadingArea"
        );

    const tableArea =
        document.getElementById(
            "categoryTableArea"
        );

    const emptyArea =
        document.getElementById(
            "categoryEmptyArea"
        );

    loadingArea.classList.remove("d-none");
    tableArea.classList.add("d-none");
    emptyArea.classList.add("d-none");

    try {
        const response =
            await fetch("/Panel/GetCategories");

        if (response.status === 401) {
            window.location.href =
                "/Account/Login";

            return;
        }

        if (response.status === 403) {
            throw new Error(
                "Bu işlem için yönetici yetkisi gereklidir."
            );
        }

        if (!response.ok) {
            throw new Error(
                "Kategoriler alınamadı."
            );
        }

        allCategories =
            await response.json();

        applyCategoryFilters();
    }
    catch (error) {
        showCategoryPageMessage(
            "danger",
            error.message ??
            "Kategoriler yüklenirken hata oluştu."
        );
    }
    finally {
        loadingArea.classList.add("d-none");
    }
}

/*
 * Fills the department filter above the category table.
 */
function renderDepartmentFilter() {
    const filterSelect =
        document.getElementById(
            "categoryDepartmentFilter"
        );

    const currentValue =
        filterSelect.value;

    filterSelect.innerHTML = `
        <option value="">
            Tüm departmanlar
        </option>
    `;

    allDepartments
        .slice()
        .sort((first, second) =>
            first.name.localeCompare(
                second.name,
                "tr-TR"
            )
        )
        .forEach(department => {
            const option =
                document.createElement("option");

            option.value =
                String(department.id);

            option.textContent =
                department.name;

            filterSelect.appendChild(option);
        });

    filterSelect.value =
        currentValue;
}

/*
 * Fills the department dropdown inside the create/update modal.
 * Inactive departments are hidden for new categories, but the current
 * department remains selectable while editing an existing category.
 */
function renderCategoryDepartmentOptions(
    selectedDepartmentId = null
) {
    const select =
        document.getElementById(
            "categoryDepartmentId"
        );

    select.innerHTML = `
        <option value="">
            Departman seçin
        </option>
    `;

    allDepartments
        .filter(department =>
            department.isActive ||
            department.id ===
                Number(selectedDepartmentId)
        )
        .slice()
        .sort((first, second) =>
            first.name.localeCompare(
                second.name,
                "tr-TR"
            )
        )
        .forEach(department => {
            const option =
                document.createElement("option");

            option.value =
                String(department.id);

            option.textContent =
                department.isActive
                    ? department.name
                    : `${department.name} (Pasif)`;

            select.appendChild(option);
        });

    if (selectedDepartmentId) {
        select.value =
            String(selectedDepartmentId);
    }
}

/*
 * Applies text and department filters without sending another API request.
 */
function applyCategoryFilters() {
    const searchText =
        document
            .getElementById(
                "categorySearch"
            )
            .value
            .trim()
            .toLocaleLowerCase("tr-TR");

    const departmentId =
        document.getElementById(
            "categoryDepartmentFilter"
        ).value;

    const filteredCategories =
        allCategories.filter(category => {
            const searchableText = [
                category.name,
                category.code,
                category.description,
                category.departmentName
            ]
                .filter(Boolean)
                .join(" ")
                .toLocaleLowerCase("tr-TR");

            const matchesSearch =
                !searchText ||
                searchableText.includes(
                    searchText
                );

            const matchesDepartment =
                !departmentId ||
                String(category.departmentId) ===
                    departmentId;

            return (
                matchesSearch &&
                matchesDepartment
            );
        });

    renderCategories(
        filteredCategories
    );
}

/*
 * Clears both category filters and restores the complete list.
 */
function clearCategoryFilters() {
    document.getElementById(
        "categorySearch"
    ).value = "";

    document.getElementById(
        "categoryDepartmentFilter"
    ).value = "";

    renderCategories(
        allCategories
    );
}

/*
 * Renders category rows into the table.
 */
function renderCategories(categories) {
    const tableBody =
        document.getElementById(
            "categoryTableBody"
        );

    const tableArea =
        document.getElementById(
            "categoryTableArea"
        );

    const emptyArea =
        document.getElementById(
            "categoryEmptyArea"
        );

    tableBody.innerHTML = "";

    if (!categories ||
        categories.length === 0) {

        tableArea.classList.add(
            "d-none"
        );

        emptyArea.classList.remove(
            "d-none"
        );

        return;
    }

    categories.forEach(category => {
        const row =
            document.createElement("tr");

        row.innerHTML = `
            <td>
                <strong>
                    ${escapeHtml(category.name)}
                </strong>

                <div class="text-muted small">
                    ${escapeHtml(category.code)}
                </div>

                ${
                    category.description
                        ? `
                            <div class="text-muted small mt-1">
                                ${escapeHtml(category.description)}
                            </div>
                        `
                        : ""
                }
            </td>

            <td>
                ${escapeHtml(
                    category.departmentName ??
                    "-"
                )}
            </td>

            <td>
                <button
                    type="button"
                    class="btn btn-sm ${
                        category.isActive
                            ? "btn-success"
                            : "btn-danger"
                    }"
                    onclick="toggleCategory(
                        ${category.id}
                    )">

                    ${
                        category.isActive
                            ? "Aktif"
                            : "Pasif"
                    }
                </button>
            </td>

            <td class="text-end">
                <button
                    type="button"
                    class="btn btn-sm btn-outline-primary"
                    onclick="editCategory(
                        ${category.id}
                    )">

                    Düzenle
                </button>
            </td>
        `;

        tableBody.appendChild(row);
    });

    emptyArea.classList.add(
        "d-none"
    );

    tableArea.classList.remove(
        "d-none"
    );
}

/*
 * Resets and opens the modal for a new category.
 */
function openCreateCategoryModal() {
    document.getElementById(
        "categoryId"
    ).value = "";

    document.getElementById(
        "categoryName"
    ).value = "";

    document.getElementById(
        "categoryCode"
    ).value = "";

    document.getElementById(
        "categoryDescription"
    ).value = "";

    renderCategoryDepartmentOptions();

    document.getElementById(
        "categoryDepartmentId"
    ).value = "";

    document.getElementById(
        "categoryModalTitle"
    ).textContent =
        "Yeni Kategori";

    hideCategoryModalMessage();

    bootstrap.Modal
        .getOrCreateInstance(
            document.getElementById(
                "categoryModal"
            )
        )
        .show();
}

/*
 * Fills and opens the modal for the selected category.
 */
function editCategory(id) {
    const category =
        allCategories.find(
            item => item.id === id
        );

    if (!category) {
        showCategoryPageMessage(
            "danger",
            "Kategori bulunamadı."
        );

        return;
    }

    document.getElementById(
        "categoryId"
    ).value =
        category.id;

    document.getElementById(
        "categoryName"
    ).value =
        category.name ?? "";

    document.getElementById(
        "categoryCode"
    ).value =
        category.code ?? "";

    document.getElementById(
        "categoryDescription"
    ).value =
        category.description ?? "";

    renderCategoryDepartmentOptions(
        category.departmentId
    );

    document.getElementById(
        "categoryModalTitle"
    ).textContent =
        "Kategoriyi Düzenle";

    hideCategoryModalMessage();

    bootstrap.Modal
        .getOrCreateInstance(
            document.getElementById(
                "categoryModal"
            )
        )
        .show();
}

/*
 * Creates a category when categoryId is empty,
 * otherwise updates the existing category.
 */
async function saveCategory() {
    const id =
        Number(
            document.getElementById(
                "categoryId"
            ).value
        );

    const name =
        document.getElementById(
            "categoryName"
        ).value.trim();

    const code =
        document.getElementById(
            "categoryCode"
        ).value.trim();

    const departmentId =
        Number(
            document.getElementById(
                "categoryDepartmentId"
            ).value
        );

    const description =
        document.getElementById(
            "categoryDescription"
        ).value.trim();

    if (!name ||
        !code ||
        !departmentId) {

        showCategoryModalMessage(
            "warning",
            "Kategori adı, kodu ve departman zorunludur."
        );

        return;
    }

    const isUpdate =
        Number.isInteger(id) &&
        id > 0;

    const existingCategory =
        allCategories.find(
            item => item.id === id
        );

    const url =
        isUpdate
            ? `/Panel/UpdateCategory?id=${id}`
            : "/Panel/CreateCategory";

    const method =
        isUpdate
            ? "PUT"
            : "POST";

    const body =
        isUpdate
            ? {
                name,
                code,
                description:
                    description || null,
                departmentId,
                isActive:
                    existingCategory
                        ?.isActive ?? true
            }
            : {
                name,
                code,
                description:
                    description || null,
                departmentId
            };

    const saveButton =
        document.getElementById(
            "saveCategoryButton"
        );

    saveButton.disabled = true;

    hideCategoryModalMessage();

    try {
        const response = await fetch(
            url,
            {
                method,
                headers: {
                    "Content-Type":
                        "application/json"
                },
                body:
                    JSON.stringify(body)
            }
        );

        if (response.status === 401) {
            window.location.href =
                "/Account/Login";

            return;
        }

        if (response.status === 403) {
            throw new Error(
                "Bu işlem için yönetici yetkisi gereklidir."
            );
        }

        if (!response.ok) {
            const apiMessage =
                await readResponseMessage(
                    response
                );

            throw new Error(
                apiMessage ||
                (
                    isUpdate
                        ? "Kategori güncellenemedi."
                        : "Kategori oluşturulamadı."
                )
            );
        }

        bootstrap.Modal
            .getInstance(
                document.getElementById(
                    "categoryModal"
                )
            )
            ?.hide();

        showCategoryPageMessage(
            "success",
            isUpdate
                ? "Kategori başarıyla güncellendi."
                : "Kategori başarıyla oluşturuldu."
        );

        await loadCategories();
    }
    catch (error) {
        showCategoryModalMessage(
            "danger",
            error.message ??
            "İşlem sırasında hata oluştu."
        );
    }
    finally {
        saveButton.disabled = false;
    }
}

/*
 * Uses the existing update endpoint to change only IsActive
 * while preserving the other category fields.
 */
async function toggleCategory(id) {
    const category =
        allCategories.find(
            item => item.id === id
        );

    if (!category) {
        showCategoryPageMessage(
            "danger",
            "Kategori bulunamadı."
        );

        return;
    }

    const body = {
        name:
            category.name,

        code:
            category.code,

        description:
            category.description,

        departmentId:
            category.departmentId,

        isActive:
            !category.isActive
    };

    try {
        const response = await fetch(
            `/Panel/UpdateCategory?id=${id}`,
            {
                method: "PUT",
                headers: {
                    "Content-Type":
                        "application/json"
                },
                body:
                    JSON.stringify(body)
            }
        );

        if (response.status === 401) {
            window.location.href =
                "/Account/Login";

            return;
        }

        if (response.status === 403) {
            throw new Error(
                "Bu işlem için yönetici yetkisi gereklidir."
            );
        }

        if (!response.ok) {
            const apiMessage =
                await readResponseMessage(
                    response
                );

            throw new Error(
                apiMessage ||
                "Kategori durumu değiştirilemedi."
            );
        }

        showCategoryPageMessage(
            "success",
            category.isActive
                ? "Kategori pasif yapıldı."
                : "Kategori aktif yapıldı."
        );

        await loadCategories();
    }
    catch (error) {
        showCategoryPageMessage(
            "danger",
            error.message ??
            "İşlem sırasında hata oluştu."
        );
    }
}

function showCategoryPageMessage(
    type,
    message
) {
    const messageArea =
        document.getElementById(
            "categoryPageMessage"
        );

    messageArea.className =
        `alert alert-${type}`;

    messageArea.textContent =
        message;
}

function showCategoryModalMessage(
    type,
    message
) {
    const messageArea =
        document.getElementById(
            "categoryModalMessage"
        );

    messageArea.className =
        `alert alert-${type}`;

    messageArea.textContent =
        message;
}

function hideCategoryModalMessage() {
    const messageArea =
        document.getElementById(
            "categoryModalMessage"
        );

    messageArea.className =
        "alert d-none";

    messageArea.textContent =
        "";
}

/*
 * Tries to read a useful error message from a JSON or text response.
 */
async function readResponseMessage(
    response
) {
    const contentType =
        response.headers.get(
            "content-type"
        ) ?? "";

    if (contentType.includes(
        "application/json"
    )) {
        const json =
            await response.json();

        return (
            json.message ??
            json.title ??
            json.detail ??
            ""
        );
    }

    return await response.text();
}

function escapeHtml(value) {
    return String(value ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}
