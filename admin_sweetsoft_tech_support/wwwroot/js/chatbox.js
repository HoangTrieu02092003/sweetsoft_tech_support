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

            chatOverlay.classList.remove("hidden");
            setTimeout(() => chatOverlay.classList.add("visible"), 10);
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
});