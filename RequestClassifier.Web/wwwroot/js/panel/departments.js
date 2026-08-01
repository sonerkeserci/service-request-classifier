let allDepartments = [];

document.addEventListener(
    "DOMContentLoaded",
    loadDepartments);

async function loadDepartments() {

    const response =
        await fetch("/Panel/GetDepartments");

    const departments =
        await response.json();
    allDepartments = departments;

    renderDepartments(departments);

    document
        .getElementById("departmentSearch")
        .addEventListener("input", filterDepartments);
}


function filterDepartments() {

    const text =
        document
            .getElementById("departmentSearch")
            .value
            .toLowerCase();

    const filtered =
        allDepartments.filter(department =>
            department.name
                .toLowerCase()
                .includes(text));

    renderDepartments(filtered);
}
function renderDepartments(departments) {

    const tbody =
        document.getElementById(
            "departmentTableBody");

    tbody.innerHTML = "";

    departments.forEach(department => {

        tbody.innerHTML += `
        <tr>

            <td>${department.name}</td>

            <td>

               ${
            department.isActive
                ? `
            <button
                class="btn btn-sm btn-success"
                onclick="toggleDepartment(${department.id})">

                Aktif

            </button>
        `
                : `
            <button
                class="btn btn-sm btn-danger"
                onclick="toggleDepartment(${department.id})">

                Pasif

            </button>
        `
}

            </td>

            <td>
                <button
                    class="btn btn-sm btn-outline-primary"
                    onclick="editDepartment(${department.id})">

                    Düzenle

                </button>

            </td>

        </tr>`;
    });

}
function editDepartment(id) {

    console.log(id);

}

async function toggleDepartment(id) {

    console.log(id);

}