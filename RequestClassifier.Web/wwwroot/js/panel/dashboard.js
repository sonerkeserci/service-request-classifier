document.addEventListener(
    "DOMContentLoaded",
    loadDashboard
);

async function loadDashboard() {
    const refreshButton =
        document.getElementById(
            "refreshDashboardButton"
        );

    refreshButton.disabled = true;
    hideDashboardMessage();

    setDashboardValues(
        "-",
        "-",
        "-",
        "-"
    );

    try {
        const [
            requests,
            employees,
            departments
        ] = await Promise.all([
            getDashboardData(
                "/Panel/GetRequests",
                "Talepler alınamadı."
            ),

            getDashboardData(
                "/Panel/GetEmployees",
                "Personeller alınamadı."
            ),

            getDashboardData(
                "/Panel/GetDepartments",
                "Departmanlar alınamadı."
            )
        ]);

        const totalRequestCount =
            Array.isArray(requests)
                ? requests.length
                : 0;

        const pendingRequestCount =
            Array.isArray(requests)
                ? requests.filter(
                    isPendingRequest
                ).length
                : 0;

        const employeeCount =
            Array.isArray(employees)
                ? employees.filter(
                    employee =>
                        employee.isActive !== false
                ).length
                : 0;

        const departmentCount =
            Array.isArray(departments)
                ? departments.filter(
                    department =>
                        department.isActive !== false
                ).length
                : 0;

        setDashboardValues(
            totalRequestCount,
            pendingRequestCount,
            employeeCount,
            departmentCount
        );
    }
    catch (error) {
        showDashboardMessage(
            "danger",
            error.message ??
            "Gösterge paneli yüklenirken hata oluştu."
        );
    }
    finally {
        refreshButton.disabled = false;
    }
}

async function getDashboardData(
    url,
    fallbackMessage
) {
    const response =
        await fetch(url);

    if (response.status === 401) {
        window.location.href =
            "/Account/Login";

        throw new Error(
            "Oturum süresi doldu."
        );
    }

    if (response.status === 403) {
        throw new Error(
            "Bu ekran için yönetici yetkisi gereklidir."
        );
    }

    if (!response.ok) {
        const apiMessage =
            await readDashboardError(
                response
            );

        throw new Error(
            apiMessage ||
            fallbackMessage
        );
    }

    return await response.json();
}

/*
 * RequestStatus enum sırası:
 * 0 = Received
 * 1 = Classified
 * 2 = Assigned
 * 3 = InProgress
 * 4 = Completed
 * 5 = Rejected
 *
 * API enum değerini metin veya sayı olarak gönderse de çalışır.
 */
function isPendingRequest(request) {
    const rawStatus =
        request.statusName ??
        request.status;

    if (typeof rawStatus === "number") {
        return (
            rawStatus >= 0 &&
            rawStatus <= 3
        );
    }

    const normalizedStatus =
        String(rawStatus ?? "")
            .replaceAll(" ", "")
            .replaceAll("_", "")
            .replaceAll("-", "")
            .toLocaleLowerCase("en-US");

    return [
        "received",
        "classified",
        "assigned",
        "inprogress"
    ].includes(normalizedStatus);
}

function setDashboardValues(
    totalRequests,
    pendingRequests,
    employees,
    departments
) {
    document.getElementById(
        "totalRequestCount"
    ).textContent =
        totalRequests;

    document.getElementById(
        "pendingRequestCount"
    ).textContent =
        pendingRequests;

    document.getElementById(
        "employeeCount"
    ).textContent =
        employees;

    document.getElementById(
        "departmentCount"
    ).textContent =
        departments;
}

function showDashboardMessage(
    type,
    message
) {
    const messageArea =
        document.getElementById(
            "dashboardMessage"
        );

    messageArea.className =
        `alert alert-${type}`;

    messageArea.textContent =
        message;
}

function hideDashboardMessage() {
    const messageArea =
        document.getElementById(
            "dashboardMessage"
        );

    messageArea.className =
        "alert d-none";

    messageArea.textContent =
        "";
}

async function readDashboardError(
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
