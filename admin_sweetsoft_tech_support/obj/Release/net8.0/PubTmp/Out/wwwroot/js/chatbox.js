// Trong file chatbox.js
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

            chatTitle.textContent = `${title}-${customer}`;
            requestIdInput.value = requestId;
            toCustomerIdInput.value = customerId;

            chatBody.innerHTML = "<p>Đang tải...</p>";
            loadChatMessages(requestId);

            //
            markFeedbackAsRead(requestId);
            document.body.classList.add('chat-open');
            //
            chatOverlay.classList.remove("hidden");
            setTimeout(() => chatOverlay.classList.add("visible"), 10);

            // Start polling for new feedbacks
            startPolling(requestId);
        });
    });

    // Hàm tải tin nhắn chat
    function loadChatMessages(requestId) {
        fetch(`/TblSupportRequests/GetFeedbacks?requestId=${requestId}`)
            .then(response => response.text())
            .then(html => {
                chatBody.innerHTML = html;
                chatBody.scrollTop = chatBody.scrollHeight;
            })
            .catch(err => {
                chatBody.innerHTML = "<p>Lỗi khi tải phản hồi!</p>";
            });
    }

    // Xử lý đóng khung chat
    closeChatButton.addEventListener("click", function () {
        chatOverlay.classList.remove("visible");
        setTimeout(() => chatOverlay.classList.add("hidden"), 300);
        document.body.classList.remove('chat-open');

        // Stop polling when chat is closed
        stopPolling();
    });

    // Xử lý form gửi tin nhắn
    document.querySelector(".chat-form").addEventListener("submit", function (e) {
        e.preventDefault();

        const formData = new FormData(this);
        const requestId = requestIdInput.value;

        fetch("/TblSupportRequests/SendMessage", {
            method: "POST",
            body: formData
        })
            .then(response => {
                if (response.ok) {
                    // Reset form và tải lại tin nhắn
                    this.reset();
                    document.getElementById("messageInput").style.height = "24px";
                    loadChatMessages(requestId);
                } else {
                    throw new Error('Gửi tin nhắn thất bại');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('Có lỗi xảy ra khi gửi tin nhắn');
            });
    });

    // Xử lý auto-resize textarea
    const textarea = document.getElementById("messageInput");
    textarea.addEventListener("input", function () {
        this.style.height = "24px";
        const newHeight = Math.min(this.scrollHeight, 100);
        this.style.height = newHeight + "px";
    });

    // Hàm polling để tải lại tin nhắn chat mỗi 5 giây
    let pollingInterval;

    function startPolling(requestId) {
        pollingInterval = setInterval(() => loadChatMessages(requestId), 3000);
    }

    function stopPolling() {
        clearInterval(pollingInterval);
    }

});

// Hàm gọi AJAX để cập nhật trạng thái đã đọc
function markFeedbackAsRead(requestId) {
    fetch(`/TblSupportRequests/MarkAsRead?requestId=${requestId}`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "X-CSRF-TOKEN": document.querySelector('input[name="__RequestVerificationToken"]').value
        }
    })
        .then(response => {
            if (!response.ok) throw new Error("Failed to mark feedback as read");
            // Nếu thành công, xóa lớp "has-unread-feedback" khỏi button
            const button = document.querySelector(`button[data-request-id="${requestId}"]`);
            if (button) {
                button.classList.remove("blinking");
            }
        })
        .catch(error => console.error("Error marking feedback as read:", error));
}
