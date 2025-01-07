
    document.addEventListener("DOMContentLoaded", function () {
    const chatOverlay = document.getElementById("chatOverlay");
    const closeChatButton = document.getElementById("closeChat");
    const chatBody = document.getElementById("chatBody");
    const chatTitle = document.getElementById("chatTitle");
    const requestIdInput = document.getElementById("requestIdInput");
    const toCustomerIdInput = document.getElementById("ToCustomerIdInput");
        // Xử lý mở khung chat
        document.querySelectorAll(".open-chat").forEach(button => {
        button.addEventListener("click", function () {
            const requestId = this.getAttribute("data-request-id");
            const customerId = this.getAttribute("data-customer-id");
            const title = this.getAttribute("data-title");
            const customer = this.getAttribute("data-customer");

            // Cập nhật tiêu đề khung chat và ID yêu cầu
            chatTitle.textContent = `${title}-${customer}`;
            requestIdInput.value = requestId;
            toCustomerIdInput.value = customerId;

            // Xóa nội dung chat cũ và tải nội dung mới
            chatBody.innerHTML = "<p>Đang tải...</p>";
            fetch(`/TblSupportRequests/GetFeedbacks?requestId=${requestId}`)
                .then(response => response.text())
                .then(html => {
                    chatBody.innerHTML = html;
                })
                .catch(err => {
                    chatBody.innerHTML = "<p>Lỗi khi tải phản hồi!</p>";
                });

            // Hiển thị khung chat
            chatOverlay.classList.remove("hidden");
            setTimeout(() => chatOverlay.classList.add("visible"), 10); // Thêm hiệu ứng trượt
        });
        });

    // Xử lý đóng khung chat
    closeChatButton.addEventListener("click", function () {
        chatOverlay.classList.remove("visible");
            setTimeout(() => chatOverlay.classList.add("hidden"), 300); // Chờ hiệu ứng chạy xong
    });

        document.querySelectorAll('input[name="FeedbackType"]')
            .forEach(function (elem) {
                elem.addEventListener('change', function (event) {
                    var value = event.target.value;
                    var ToUserIdInputDiv = document.getElementById('select');
                    if (value === '3') {
                        ToUserIdInputDiv.style.display = 'block';
                    } else { ToUserIdInputDiv.style.display = 'none'; }
                });
            });
    });