const vietnameseMonths = [
    "Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5", "Tháng 6",
    "Tháng 7", "Tháng 8", "Tháng 9", "Tháng 10", "Tháng 11", "Tháng 12"
];

// Hàm vẽ biểu đồ với dữ liệu
async function fetchAndRenderChart(startDate, endDate, isDefault) {
    try {
        const response = await fetch(`/api/requests/monthly?startDate=${startDate}&endDate=${endDate}`);
        if (!response.ok) {
            throw new Error("Failed to fetch data from API");
        }

        const data = await response.json();

        if (!data.monthlySummary || data.monthlySummary.length === 0) {
            alert("Không có dữ liệu trong khoảng thời gian này");
            return;
        }

        let labels = [];
        let counts = [];

        if (isDefault) {
            const currentYear = new Date().getFullYear();
            for (let i = 1; i <= 12; i++) {
                labels.push(vietnameseMonths[i - 1]);
                counts.push(0);
            }
        } else {
            const start = new Date(startDate);
            const end = new Date(endDate);
            const months = getMonthsBetween(start, end);

            labels = months.map(month => vietnameseMonths[month.getMonth()]);
            counts = new Array(months.length).fill(0);

            data.monthlySummary.forEach(item => {
                const monthIndex = months.findIndex(month =>
                    month.getMonth() === item.month - 1 &&
                    month.getFullYear() === item.year
                );
                if (monthIndex !== -1) {
                    counts[monthIndex] = item.count;
                }
            });
        }

        console.log("Labels:", labels);
        console.log("Counts:", counts);

        if (myLineChart) {
            myLineChart.destroy();
        }

        drawChart(labels, counts);

    } catch (error) {
        console.error("Error fetching data:", error);
    }
}

// Hàm vẽ biểu đồ mặc định với dữ liệu của năm hiện tại
async function renderDefaultChart() {
    const currentYear = new Date().getFullYear();
    const defaultStartDate = `${currentYear}-01-01`;
    const defaultEndDate = `${currentYear}-12-31`;

    let labels = [];
    let counts = [];

    for (let i = 1; i <= 12; i++) {
        labels.push(vietnameseMonths[i - 1]);
        counts.push(0);
    }

    try {
        const response = await fetch(`/api/requests/monthly?startDate=${defaultStartDate}&endDate=${defaultEndDate}`);
        if (!response.ok) {
            throw new Error("Failed to fetch data from API");
        }

        const data = await response.json();

        if (data.monthlySummary && data.monthlySummary.length > 0) {
            data.monthlySummary.forEach(item => {
                const monthIndex = labels.findIndex(label =>
                    vietnameseMonths[item.month - 1] === label
                );
                if (monthIndex !== -1) {
                    counts[monthIndex] = item.count;
                }
            });
        }

        drawChart(labels, counts);
    } catch (error) {
        console.error("Error fetching default data:", error);
        alert("Mời bạn nhập ngày bắt đầu và kết thúc.");
    }
}

// Hàm lấy các tháng trong khoảng thời gian từ startDate đến endDate
function getMonthsBetween(startDate, endDate) {
    const months = [];
    const start = new Date(startDate);
    const end = new Date(endDate);

    while (start <= end) {
        months.push(new Date(start));
        start.setMonth(start.getMonth() + 1);
    }

    return months;
}

// Hàm vẽ biểu đồ với labels và counts
function drawChart(labels, data) {
    const ctx = document.getElementById("myAreaChart").getContext("2d");

    myLineChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: "Số yêu cầu",
                lineTension: 0.3,
                backgroundColor: "rgba(121, 28, 181, 0.05)",
                borderColor: "rgba(121, 28, 181, 1)",
                pointRadius: 3,
                pointBackgroundColor: "rgba(121, 28, 181, 1)",
                pointBorderColor: "rgba(121, 28, 181, 1)",
                pointHoverRadius: 3,
                pointHoverBackgroundColor: "rgba(121, 28, 181, 1)",
                pointHoverBorderColor: "rgba(121, 28, 181, 1)",
                pointHitRadius: 10,
                pointBorderWidth: 2,
                data: data,
            }]
        },
        options: {
            maintainAspectRatio: false,
            scales: {
                x: {
                    type: 'category',
                    labels: labels,
                    grid: { display: false },
                },
                y: {
                    beginAtZero: true,
                    ticks: {
                        stepSize: 2,
                    },
                    grid: {
                        color: "rgb(234, 236, 244)",
                    }
                }
            }
        }
    });
}

// Xử lý sự kiện nhấn nút để lọc dữ liệu
document.getElementById("filterButton").addEventListener("click", async function (event) {
    event.preventDefault(); // Ngăn không cho hành động mặc định

    // Lấy giá trị ngày bắt đầu và ngày kết thúc từ các input
    const startDate = document.getElementById("startDate").value;
    const endDate = document.getElementById("endDate").value;

    // Nếu không có ngày, hiển thị 12 tháng trong năm hiện tại
    if (!startDate || !endDate) {
        await renderDefaultChart();
    } else {
        // Kiểm tra nếu ngày bắt đầu và ngày kết thúc hợp lệ
        if (new Date(startDate) > new Date(endDate)) {
            alert("Ngày bắt đầu không thể trễ hơn ngày kết thúc");
            return;
        }
        // Gọi hàm để fetch và render biểu đồ với dữ liệu mới
        await fetchAndRenderChart(startDate, endDate, false);
    }
});

// Tự động vẽ biểu đồ mặc định khi trang tải
window.addEventListener("DOMContentLoaded", async function () {
    await renderDefaultChart();
});
