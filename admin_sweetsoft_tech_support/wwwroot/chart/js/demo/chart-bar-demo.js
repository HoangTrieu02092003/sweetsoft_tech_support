document.getElementById('filterButton').addEventListener('click', function (event) {
    event.preventDefault(); // Ngăn chặn reload trang khi nhấn nút

    // Lấy giá trị từ form ngày bắt đầu và kết thúc
    const startDate = document.getElementById("startDate").value || getCurrentYearStartDate();
    const endDate = document.getElementById("endDate").value || getCurrentYearEndDate();

    // Gọi hàm tạo biểu đồ với startDate và endDate
    createChart(startDate, endDate);
});

// Hàm lấy ngày đầu tiên của năm hiện tại
function getCurrentYearStartDate() {
    const currentYear = new Date().getFullYear();
    return `${currentYear}-01-01`; // Định dạng yyyy-mm-dd
}

// Hàm lấy ngày cuối cùng của năm hiện tại
function getCurrentYearEndDate() {
    const currentYear = new Date().getFullYear();
    return `${currentYear}-12-31`; // Định dạng yyyy-mm-dd
}

async function fetchData(startDate, endDate) {
    try {
        // Tạo URL với tham số startDate và endDate
        const url = new URL('/api/requests/status-summary', window.location.origin);
        if (startDate) url.searchParams.append('startDate', startDate);
        if (endDate) url.searchParams.append('endDate', endDate);

        const response = await fetch(url); // Gọi API từ controller mới
        const data = await response.json(); // Parse dữ liệu JSON
        console.log("API from status", data); // Kiểm tra dữ liệu trả về
        return data;
    } catch (error) {
        console.error("Error fetching data:", error);
    }
}

let myBarChart; // Khai báo biến toàn cục để lưu biểu đồ hiện tại

async function createChart(startDate, endDate) {
    const data = await fetchData(startDate, endDate);

    if (!data || !data.requests) {
        console.error("No data available for chart.");
        return;
    }

<<<<<<< HEAD
    // Tạo một mảng đếm theo trạng thái và khởi tạo với 0
=======
>>>>>>> fa464a014619a2177d227f89bddd56d069192394
    const statusCounts = { 0: 0, 1: 0, 2: 0 };

    data.requests.forEach(request => {
        if (statusCounts.hasOwnProperty(request.status)) {
            statusCounts[request.status] = request.count;
        }
    });

    const ctx = document.getElementById("myBarChart");

    if (myBarChart) {
        myBarChart.destroy();
    }

    // Tính giá trị lớn nhất của dữ liệu và thêm khoảng đệm
    const maxValue = Math.max(...Object.values(statusCounts));
    const suggestedMax = maxValue + Math.ceil(maxValue * 0.1); // Thêm 10% khoảng trống

    myBarChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: ['Chưa hoàn thành', 'Hoàn thành', 'Không xử lý được'], // Trục X
            datasets: [{
                label: "Number of Requests",
                backgroundColor: [
                    "rgba(255, 99, 132, 0.5)", // Chưa hoàn thành
                    "rgba(75, 192, 192, 0.5)", // Hoàn thành
                    "rgba(255, 159, 64, 0.5)"  // Không xử lý được
                ],
                borderColor: [
                    "rgba(255, 99, 132, 1)",
                    "rgba(75, 192, 192, 1)",
                    "rgba(255, 159, 64, 1)"
                ],
                data: [
                    statusCounts[0], // Chưa hoàn thành
                    statusCounts[1], // Hoàn thành
                    statusCounts[2]  // Không xử lý được
                ],
                borderWidth: 1
            }]
        },
        options: {
            maintainAspectRatio: false,
            scales: {
                y: {
                    beginAtZero: true,
                    suggestedMax: suggestedMax, // Tăng giới hạn trục y
                    ticks: {
                        stepSize: 2
                    }
                }
            },
            plugins: {
                legend: {
                    display: false // Tắt hiển thị chú thích
                },
                tooltip: {
                    enabled: false // Tắt tooltip
                },
                datalabels: {
                    anchor: 'end', // Vị trí hiển thị
                    align: 'end', // Căn chỉnh
                    formatter: (value) => value, // Hiển thị giá trị
                    font: {
                        weight: 'bold' // Kiểu chữ
                    }
                }
            }
        },
        plugins: [ChartDataLabels] // Bật plugin ChartDataLabels
    });
}

// Gọi createChart khi trang tải để hiển thị dữ liệu mặc định của năm hiện tại
<<<<<<< HEAD
createChart(getCurrentYearStartDate(), getCurrentYearEndDate());
=======
createChart(getCurrentYearStartDate(), getCurrentYearEndDate());

>>>>>>> fa464a014619a2177d227f89bddd56d069192394
