let allDepartments = [];

document.addEventListener(
    "DOMContentLoaded",
    async function () {
        document
            .getElementById("departmentSearch")
            .addEventListener(
                "input",
                filterDepartments
            );

        await loadDepartments();
    }
);

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

        renderDepartments(
            allDepartments
        );
    }
    catch (error) {
        alert(
            error.message ??
            "Departmanlar yüklenirken hata oluştu."
        );
    }
}

function filterDepartments() {
    const text =
        document
            .getElementById(
                "departmentSearch"
            )
            .value
            .trim()
            .toLocaleLowerCase("tr-TR");

    const filtered =
        allDepartments.filter(
            department => {
                const searchableText = [
                    department.name,
                    department.code,
                    department.description
                ]
                    .filter(Boolean)
                    .join(" ")
                    .toLocaleLowerCase("tr-TR");

                return searchableText.includes(
                    text
                );
            }
        );

    renderDepartments(filtered);
}

function renderDepartments(departments) {
    const tbody =
        document.getElementById(
            "departmentTableBody"
        );

    tbody.innerHTML = "";

    departments.forEach(department => {
        const row =
            document.createElement("tr");

        row.innerHTML = `
            <td>
                <strong>
                    ${escapeHtml(department.name)}
                </strong>

                <div class="text-muted small">
                    ${escapeHtml(department.code)}
                </div>
            </td>

            <td>
                <button
                    type="button"
                    class="btn btn-sm ${department.isActive
                ? "btn-success"
                : "btn-danger"
            }"
                    onclick="toggleDepartment(
                        ${department.id}
                    )">

                    ${department.isActive
                ? "Aktif"
                : "Pasif"
            }
                </button>
            </td>

            <td>
                <button
                    type="button"
                    class="btn btn-sm btn-outline-primary"
                    onclick="editDepartment(
                        ${department.id}
                    )">

                    Düzenle
                </button>
            </td>
        `;

        tbody.appendChild(row);
    });
}

function openCreateDepartmentModal() {
    document.getElementById(
        "departmentId"
    ).value = "";

    document.getElementById(
        "departmentName"
    ).value = "";

    document.getElementById(
        "departmentCode"
    ).value = "";

    document.getElementById(
        "departmentDescription"
    ).value = "";

    document.querySelector(
        "#departmentModal .modal-title"
    ).textContent =
        "Yeni Departman";
}

function editDepartment(id) {
    const department =
        allDepartments.find(
            item => item.id === id
        );

    if (!department) {
        alert("Departman bulunamadı.");
        return;
    }

    document.getElementById(
        "departmentId"
    ).value =
        department.id;

    document.getElementById(
        "departmentName"
    ).value =
        department.name ?? "";

    document.getElementById(
        "departmentCode"
    ).value =
        department.code ?? "";

    document.getElementById(
        "departmentDescription"
    ).value =
        department.description ?? "";

    document.querySelector(
        "#departmentModal .modal-title"
    ).textContent =
        "Departmanı Düzenle";

    const modalElement =
        document.getElementById(
            "departmentModal"
        );

    bootstrap.Modal
        .getOrCreateInstance(
            modalElement
        )
        .show();
}

async function saveDepartment() {
    const id =
        Number(
            document.getElementById(
                "departmentId"
            ).value
        );

    const name =
        document.getElementById(
            "departmentName"
        ).value.trim();

    const code =
        document.getElementById(
            "departmentCode"
        ).value.trim();

    const description =
        document.getElementById(
            "departmentDescription"
        ).value.trim();

    if (!name || !code) {
        alert(
            "Departman adı ve kodu zorunludur."
        );

        return;
    }

    const existingDepartment =
        allDepartments.find(
            item => item.id === id
        );

    const isUpdate =
        Number.isInteger(id) &&
        id > 0;

    const url =
        isUpdate
            ? `/Panel/UpdateDepartment?id=${id}`
            : "/Panel/CreateDepartment";

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
                isActive:
                    existingDepartment
                        ?.isActive ?? true
            }
            : {
                name,
                code,
                description:
                    description || null
            };

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

        if (!response.ok) {
            throw new Error(
                isUpdate
                    ? "Departman güncellenemedi."
                    : "Departman oluşturulamadı."
            );
        }

        bootstrap.Modal
            .getInstance(
                document.getElementById(
                    "departmentModal"
                )
            )
            ?.hide();

        await loadDepartments();
    }
    catch (error) {
        alert(
            error.message ??
            "İşlem sırasında hata oluştu."
        );
    }
}

async function toggleDepartment(id) {
    const department =
        allDepartments.find(
            item => item.id === id
        );

    if (!department) {
        alert("Departman bulunamadı.");
        return;
    }

    const body = {
        name:
            department.name,

        code:
            department.code,

        description:
            department.description,

        isActive:
            !department.isActive
    };

    try {
        const response = await fetch(
            `/Panel/UpdateDepartment?id=${id}`,
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

        if (!response.ok) {
            throw new Error(
                "Departman durumu değiştirilemedi."
            );
        }

        await loadDepartments();
    }
    catch (error) {
        alert(
            error.message ??
            "İşlem sırasında hata oluştu."
        );
    }
}

function escapeHtml(value) {
    return String(value ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}