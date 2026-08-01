let allEmployees = [];
let allDepartments = [];

document.addEventListener(
    "DOMContentLoaded",
    async function () {
        document
            .getElementById("employeeSearch")
            .addEventListener(
                "input",
                applyEmployeeFilters
            );

        document
            .getElementById("employeeDepartmentFilter")
            .addEventListener(
                "change",
                applyEmployeeFilters
            );

        document
            .getElementById("employeeRoleFilter")
            .addEventListener(
                "change",
                applyEmployeeFilters
            );

        await loadDepartments();
        await loadEmployees();
    }
);

/*
 * Loads departments for the table filter and employee modal.
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

        if (response.status === 403) {
            throw new Error(
                "Bu işlem için yönetici yetkisi gereklidir."
            );
        }

        if (!response.ok) {
            throw new Error(
                await readResponseMessage(response) ||
                "Departmanlar alınamadı."
            );
        }

        allDepartments =
            await response.json();

        renderEmployeeDepartmentFilter();
        renderEmployeeDepartmentOptions();
    }
    catch (error) {
        showEmployeePageMessage(
            "danger",
            error.message ??
            "Departmanlar yüklenirken hata oluştu."
        );
    }
}

/*
 * Loads all administrators and employees.
 */
async function loadEmployees() {
    const loadingArea =
        document.getElementById(
            "employeeLoadingArea"
        );

    const tableArea =
        document.getElementById(
            "employeeTableArea"
        );

    const emptyArea =
        document.getElementById(
            "employeeEmptyArea"
        );

    loadingArea.classList.remove("d-none");
    tableArea.classList.add("d-none");
    emptyArea.classList.add("d-none");

    try {
        const response =
            await fetch("/Panel/GetEmployees");

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
                await readResponseMessage(response) ||
                "Personeller alınamadı."
            );
        }

        allEmployees =
            await response.json();

        applyEmployeeFilters();
    }
    catch (error) {
        showEmployeePageMessage(
            "danger",
            error.message ??
            "Personeller yüklenirken hata oluştu."
        );
    }
    finally {
        loadingArea.classList.add("d-none");
    }
}

/*
 * Fills the department filter above the employee table.
 */
function renderEmployeeDepartmentFilter() {
    const select =
        document.getElementById(
            "employeeDepartmentFilter"
        );

    const currentValue =
        select.value;

    select.innerHTML = `
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

            select.appendChild(option);
        });

    select.value =
        currentValue;
}

/*
 * Fills the department dropdown in the employee modal.
 * Inactive departments are hidden when creating a new employee.
 * While editing, the employee's current department remains visible.
 */
function renderEmployeeDepartmentOptions(
    selectedDepartmentId = null
) {
    const select =
        document.getElementById(
            "employeeDepartmentId"
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
 * Applies search, department and role filters locally.
 */
function applyEmployeeFilters() {
    const searchText =
        document
            .getElementById(
                "employeeSearch"
            )
            .value
            .trim()
            .toLocaleLowerCase("tr-TR");

    const departmentId =
        document.getElementById(
            "employeeDepartmentFilter"
        ).value;

    const role =
        document.getElementById(
            "employeeRoleFilter"
        ).value;

    const filteredEmployees =
        allEmployees.filter(employee => {
            const searchableText = [
                employee.firstName,
                employee.lastName,
                employee.email,
                employee.departmentName,
                getEmployeeRoleLabel(
                    employee.role
                )
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
                String(employee.departmentId ?? "") ===
                    departmentId;

            const matchesRole =
                !role ||
                employee.role === role;

            return (
                matchesSearch &&
                matchesDepartment &&
                matchesRole
            );
        });

    renderEmployees(
        filteredEmployees
    );
}

/*
 * Clears all employee filters.
 */
function clearEmployeeFilters() {
    document.getElementById(
        "employeeSearch"
    ).value = "";

    document.getElementById(
        "employeeDepartmentFilter"
    ).value = "";

    document.getElementById(
        "employeeRoleFilter"
    ).value = "";

    renderEmployees(
        allEmployees
    );
}

/*
 * Renders employee rows.
 */
function renderEmployees(employees) {
    const tableBody =
        document.getElementById(
            "employeeTableBody"
        );

    const tableArea =
        document.getElementById(
            "employeeTableArea"
        );

    const emptyArea =
        document.getElementById(
            "employeeEmptyArea"
        );

    tableBody.innerHTML = "";

    if (!employees ||
        employees.length === 0) {

        tableArea.classList.add(
            "d-none"
        );

        emptyArea.classList.remove(
            "d-none"
        );

        return;
    }

    employees.forEach(employee => {
        const row =
            document.createElement("tr");

        const fullName =
            `${employee.firstName ?? ""} ${employee.lastName ?? ""}`
                .trim();

        const roleLabel =
            getEmployeeRoleLabel(
                employee.role
            );

        const departmentText =
            employee.role === "Admin"
                ? "Sistem geneli"
                : (
                    employee.departmentName ??
                    "Departman atanmamış"
                );

        row.innerHTML = `
            <td>
                <strong>
                    ${escapeHtml(fullName || "-")}
                </strong>

                <div class="text-muted small">
                    ${escapeHtml(employee.email)}
                </div>
            </td>

            <td>
                <span class="badge ${
                    employee.role === "Admin"
                        ? "text-bg-dark"
                        : "text-bg-primary"
                }">
                    ${escapeHtml(roleLabel)}
                </span>
            </td>

            <td>
                ${escapeHtml(departmentText)}
            </td>

            <td>
                <button
                    type="button"
                    class="btn btn-sm ${
                        employee.isActive
                            ? "btn-success"
                            : "btn-danger"
                    }"
                    onclick="toggleEmployee(
                        '${escapeJavaScriptString(employee.id)}'
                    )">

                    ${
                        employee.isActive
                            ? "Aktif"
                            : "Pasif"
                    }
                </button>
            </td>

            <td class="text-end">
                <button
                    type="button"
                    class="btn btn-sm btn-outline-primary"
                    onclick="editEmployee(
                        '${escapeJavaScriptString(employee.id)}'
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
 * Resets and opens the modal for a new person.
 */
function openCreateEmployeeModal() {
    document.getElementById(
        "employeeId"
    ).value = "";

    document.getElementById(
        "employeeFirstName"
    ).value = "";

    document.getElementById(
        "employeeLastName"
    ).value = "";

    document.getElementById(
        "employeeEmail"
    ).value = "";

    document.getElementById(
        "employeePassword"
    ).value = "";

    document.getElementById(
        "employeePassword"
    ).type = "password";

    document.getElementById(
        "toggleEmployeePasswordButton"
    ).textContent = "Göster";

    document.getElementById(
        "employeeRole"
    ).value = "";

    document.getElementById(
        "employeeRole"
    ).disabled = false;

    renderEmployeeDepartmentOptions();

    document.getElementById(
        "employeeDepartmentId"
    ).value = "";

    document.getElementById(
        "employeeDepartmentId"
    ).disabled = false;

    document.getElementById(
        "employeePasswordArea"
    ).classList.remove("d-none");

    document.getElementById(
        "employeeModalTitle"
    ).textContent =
        "Yeni Personel";

    hideEmployeeModalMessage();
    handleEmployeeRoleChange();

    bootstrap.Modal
        .getOrCreateInstance(
            document.getElementById(
                "employeeModal"
            )
        )
        .show();
}

/*
 * Opens the selected employee for editing.
 * The current backend update DTO does not contain Role or Password,
 * therefore these fields cannot be changed during update.
 */
function editEmployee(id) {
    const employee =
        allEmployees.find(
            item => item.id === id
        );

    if (!employee) {
        showEmployeePageMessage(
            "danger",
            "Personel bulunamadı."
        );

        return;
    }

    document.getElementById(
        "employeeId"
    ).value =
        employee.id;

    document.getElementById(
        "employeeFirstName"
    ).value =
        employee.firstName ?? "";

    document.getElementById(
        "employeeLastName"
    ).value =
        employee.lastName ?? "";

    document.getElementById(
        "employeeEmail"
    ).value =
        employee.email ?? "";

    document.getElementById(
        "employeeRole"
    ).value =
        employee.role ?? "";

    document.getElementById(
        "employeeRole"
    ).disabled = true;

    renderEmployeeDepartmentOptions(
        employee.departmentId
    );

    document.getElementById(
        "employeePasswordArea"
    ).classList.add("d-none");

    document.getElementById(
        "employeeModalTitle"
    ).textContent =
        "Personeli Düzenle";

    hideEmployeeModalMessage();
    handleEmployeeRoleChange();

    bootstrap.Modal
        .getOrCreateInstance(
            document.getElementById(
                "employeeModal"
            )
        )
        .show();
}

/*
 * Enables department selection only for the Employee role.
 * Admin accounts are system-wide and do not require a department.
 */
function handleEmployeeRoleChange() {
    const role =
        document.getElementById(
            "employeeRole"
        ).value;

    const departmentSelect =
        document.getElementById(
            "employeeDepartmentId"
        );

    const departmentHelp =
        document.getElementById(
            "employeeDepartmentHelp"
        );

    const roleHelp =
        document.getElementById(
            "employeeRoleHelp"
        );

    if (role === "Admin") {
        departmentSelect.value = "";
        departmentSelect.disabled = true;

        departmentHelp.textContent =
            "Sistem yöneticisi herhangi bir departmana bağlı değildir.";

        roleHelp.textContent =
            "Sistem yöneticisi tüm yönetim ekranlarına erişebilir.";

        return;
    }

    departmentSelect.disabled = false;

    if (role === "Employee") {
        departmentHelp.textContent =
            "Birim personeli için departman seçimi zorunludur.";

        roleHelp.textContent =
            "Birim personeli yalnızca bağlı olduğu birimin taleplerini görür.";

        return;
    }

    departmentHelp.textContent =
        "Birim personeli için departman seçimi zorunludur.";

    roleHelp.textContent =
        "Personelin sistemdeki yetki seviyesini seçin.";
}

/*
 * Creates a new employee or updates an existing employee.
 */
async function saveEmployee() {
    const id =
        document.getElementById(
            "employeeId"
        ).value.trim();

    const firstName =
        document.getElementById(
            "employeeFirstName"
        ).value.trim();

    const lastName =
        document.getElementById(
            "employeeLastName"
        ).value.trim();

    const email =
        document.getElementById(
            "employeeEmail"
        ).value.trim();

    const password =
        document.getElementById(
            "employeePassword"
        ).value;

    const role =
        document.getElementById(
            "employeeRole"
        ).value;

    const departmentValue =
        document.getElementById(
            "employeeDepartmentId"
        ).value;

    const departmentId =
        departmentValue
            ? Number(departmentValue)
            : null;

    const isUpdate =
        id.length > 0;

    const existingEmployee =
        allEmployees.find(
            item => item.id === id
        );

    if (!firstName ||
        !lastName ||
        !email) {

        showEmployeeModalMessage(
            "warning",
            "Ad, soyad ve e-posta zorunludur."
        );

        return;
    }

    if (!isValidEmail(email)) {
        showEmployeeModalMessage(
            "warning",
            "Geçerli bir e-posta adresi girin."
        );

        return;
    }

    if (!isUpdate) {
        if (!password) {
            showEmployeeModalMessage(
                "warning",
                "Yeni personel için geçici şifre zorunludur."
            );

            return;
        }

        if (password.length < 6) {
            showEmployeeModalMessage(
                "warning",
                "Geçici şifre en az 6 karakter olmalıdır."
            );

            return;
        }

        if (!role) {
            showEmployeeModalMessage(
                "warning",
                "Personel rolü seçilmelidir."
            );

            return;
        }

        if (role === "Employee" &&
            !departmentId) {

            showEmployeeModalMessage(
                "warning",
                "Birim personeli için departman seçilmelidir."
            );

            return;
        }
    }

    const updateRole =
        existingEmployee?.role ??
        role;

    if (isUpdate &&
        updateRole === "Employee" &&
        !departmentId) {

        showEmployeeModalMessage(
            "warning",
            "Birim personeli için departman seçilmelidir."
        );

        return;
    }

    const url =
        isUpdate
            ? `/Panel/UpdateEmployee?id=${encodeURIComponent(id)}`
            : "/Panel/CreateEmployee";

    const method =
        isUpdate
            ? "PUT"
            : "POST";

    const body =
        isUpdate
            ? {
                firstName,
                lastName,
                email,
                departmentId:
                    updateRole === "Admin"
                        ? null
                        : departmentId,
                isActive:
                    existingEmployee
                        ?.isActive ?? true
            }
            : {
                firstName,
                lastName,
                email,
                password,
                role,
                departmentId:
                    role === "Admin"
                        ? null
                        : departmentId
            };

    const saveButton =
        document.getElementById(
            "saveEmployeeButton"
        );

    saveButton.disabled = true;
    hideEmployeeModalMessage();

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
                        ? "Personel güncellenemedi."
                        : "Personel oluşturulamadı."
                )
            );
        }

        bootstrap.Modal
            .getInstance(
                document.getElementById(
                    "employeeModal"
                )
            )
            ?.hide();

        showEmployeePageMessage(
            "success",
            isUpdate
                ? "Personel başarıyla güncellendi."
                : "Personel başarıyla oluşturuldu."
        );

        await loadEmployees();
    }
    catch (error) {
        showEmployeeModalMessage(
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
 * Activates or deactivates an employee using the existing update endpoint.
 */
async function toggleEmployee(id) {
    const employee =
        allEmployees.find(
            item => item.id === id
        );

    if (!employee) {
        showEmployeePageMessage(
            "danger",
            "Personel bulunamadı."
        );

        return;
    }

    const body = {
        firstName:
            employee.firstName,

        lastName:
            employee.lastName,

        email:
            employee.email,

        departmentId:
            employee.role === "Admin"
                ? null
                : employee.departmentId,

        isActive:
            !employee.isActive
    };

    try {
        const response = await fetch(
            `/Panel/UpdateEmployee?id=${encodeURIComponent(id)}`,
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
            throw new Error(
                await readResponseMessage(response) ||
                "Personel durumu değiştirilemedi."
            );
        }

        showEmployeePageMessage(
            "success",
            employee.isActive
                ? "Personel pasif yapıldı."
                : "Personel aktif yapıldı."
        );

        await loadEmployees();
    }
    catch (error) {
        showEmployeePageMessage(
            "danger",
            error.message ??
            "İşlem sırasında hata oluştu."
        );
    }
}

function toggleEmployeePasswordVisibility() {
    const passwordInput =
        document.getElementById(
            "employeePassword"
        );

    const button =
        document.getElementById(
            "toggleEmployeePasswordButton"
        );

    if (passwordInput.type === "password") {
        passwordInput.type = "text";
        button.textContent = "Gizle";
    }
    else {
        passwordInput.type = "password";
        button.textContent = "Göster";
    }
}

function getEmployeeRoleLabel(role) {
    if (role === "Admin") {
        return "Sistem Yöneticisi";
    }

    if (role === "Employee") {
        return "Birim Personeli";
    }

    return role || "Rol atanmamış";
}

function showEmployeePageMessage(
    type,
    message
) {
    const messageArea =
        document.getElementById(
            "employeePageMessage"
        );

    messageArea.className =
        `alert alert-${type}`;

    messageArea.textContent =
        message;
}

function showEmployeeModalMessage(
    type,
    message
) {
    const messageArea =
        document.getElementById(
            "employeeModalMessage"
        );

    messageArea.className =
        `alert alert-${type}`;

    messageArea.textContent =
        message;
}

function hideEmployeeModalMessage() {
    const messageArea =
        document.getElementById(
            "employeeModalMessage"
        );

    messageArea.className =
        "alert d-none";

    messageArea.textContent =
        "";
}

/*
 * Reads a useful message from API or PanelController error responses.
 */
async function readResponseMessage(
    response
) {
    const text =
        await response.text();

    if (!text) {
        return "";
    }

    try {
        const json =
            JSON.parse(text);

        if (json.errors) {
            const validationMessages =
                Object.values(json.errors)
                    .flat()
                    .filter(Boolean);

            if (validationMessages.length > 0) {
                return validationMessages.join("\n");
            }
        }

        if (json.detail) {
            try {
                const detailJson =
                    JSON.parse(json.detail);

                return (
                    detailJson.message ??
                    detailJson.title ??
                    json.message ??
                    json.title ??
                    json.detail
                );
            }
            catch {
                return (
                    json.message ??
                    json.title ??
                    json.detail
                );
            }
        }

        return (
            json.message ??
            json.title ??
            json.detail ??
            text
        );
    }
    catch {
        return text;
    }
}

function isValidEmail(value) {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/
        .test(value);
}

function escapeHtml(value) {
    return String(value ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}

function escapeJavaScriptString(value) {
    return String(value ?? "")
        .replaceAll("\\", "\\\\")
        .replaceAll("'", "\\'");
}
