let departmentPieChart; // Biến toàn cục lưu biểu đồ tròn

// Hàm lấy dữ liệu từ API
async function fetchDepartmentData(startDate, endDate) {
    try {
        const url = new URL('/api/requests/department-summary', window.location.origin);
        if (startDate) url.searchParams.append('startDate', startDate);
        if (endDate) url.searchParams.append('endDate', endDate);

        const response = await fetch(url); // Gọi API
        const data = await response.json(); // Parse dữ liệu JSON
        console.log("API Response:", data); // Kiểm tra dữ liệu trả về
        return data.departmentPercentages || []; // Trả về mảng phần trăm phòng ban
    } catch (error) {
        console.error("Error fetching department data:", error);
        return [];
    }
}

// Hàm tạo hoặc cập nhật biểu đồ tròn
async function createOrUpdatePieChart(startDate, endDate) {
    const departmentData = await fetchDepartmentData(startDate, endDate);

    // Kiểm tra dữ liệu trả về
    if (!departmentData.length) {
        console.error("No department data available for chart.");
        return;
    }

    // Chuẩn bị dữ liệu cho biểu đồ
    const labels = departmentData.map(d => d.departmentName); // Tên phòng ban
    const data = departmentData.map(d => d.percentage); // Phần trăm
    console.log("Labels:", labels);
    console.log("Data:", data);

    const backgroundColors = ['#4e73df', '#1cc88a', '#36b9cc', '#f6c23e', '#e74a3b', '#858796', '#f8f9fc', '#5a5c69'];
    const hoverBackgroundColors = ['#2e59d9', '#17a673', '#2c9faf', '#f4b619', '#e02d1b', '#6c757d', '#e9ecef', '#4e4e50'];

    // Tìm canvas trong DOM
    const ctx = document.getElementById("myPieChart");
    if (!ctx) {
        console.error("Canvas element with id 'myPieChart' not found.");
        return;
    }

    // Kiểm tra nếu biểu đồ đã tồn tại, phá hủy trước khi tạo mới
    if (departmentPieChart) {
        departmentPieChart.destroy();
    }

    // Tạo biểu đồ mới
    departmentPieChart = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: data,
                backgroundColor: backgroundColors,
                hoverBackgroundColor: hoverBackgroundColors,
                hoverBorderColor: "rgba(234, 236, 244, 1)",
            }]
        },
        options: {
            maintainAspectRatio: false,
            tooltips: {
                backgroundColor: "rgb(255,255,255)",
                bodyFontColor: "#858796",
                borderColor: '#dddfeb',
                borderWidth: 1,
                xPadding: 15,
                yPadding: 15,
                displayColors: false,
                caretPadding: 10,
            },
            legend: {
                display: true,
                position: 'bottom', 
                labels: {
                    fontColor: '#333',
                    fontSize: 12,
                    boxWidth: 20
                }
            },
            cutoutPercentage: 70 // Tùy chỉnh phần rỗng bên trong
        }
    });
}

// Sự kiện lọc dữ liệu bằng nút
document.getElementById('filterButton').addEventListener('click', async function (event) {
    event.preventDefault(); // Ngăn chặn reload trang

    // Lấy giá trị ngày bắt đầu và kết thúc từ form
    const startDate = document.getElementById("startDate").value || getCurrentYearStartDate();
    const endDate = document.getElementById("endDate").value || getCurrentYearEndDate();

    // Tạo hoặc cập nhật biểu đồ tròn
    await createOrUpdatePieChart(startDate, endDate);
});

// Hàm trả về ngày đầu tiên của năm hiện tại
function getCurrentYearStartDate() {
    const currentYear = new Date().getFullYear();
    return `${currentYear}-01-01`;
}

// Hàm trả về ngày cuối cùng của năm hiện tại
function getCurrentYearEndDate() {
    const currentYear = new Date().getFullYear();
    return `${currentYear}-12-31`;
}

// Gọi hàm tạo biểu đồ khi tải trang với dữ liệu mặc định của năm hiện tại
createOrUpdatePieChart(getCurrentYearStartDate(), getCurrentYearEndDate());
