(function () {
    'use strict';

    const sessionId = document.getElementById('sessionId')?.value;
    const messageInput = document.getElementById('messageInput');
    const sendBtn = document.getElementById('sendBtn');
    const chatForm = document.getElementById('chatForm');
    const messagesContainer = document.getElementById('messagesContainer');
    const messagesList = document.getElementById('messagesList');
    const typingIndicator = document.getElementById('typingIndicator');

    if (!sessionId || !messageInput || !chatForm) {
        return;
    }

    const antiForgeryToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    chatForm.addEventListener('submit', handleSubmit);
    messageInput.addEventListener('keydown', handleKeyDown);
    messageInput.addEventListener('input', autoResize);

    scrollToBottom();

    async function handleSubmit(e) {
        e.preventDefault();

        const content = messageInput.value.trim();
        if (!content) {
            return;
        }

        appendMessage('USER', content);
        messageInput.value = '';
        autoResize();
        setLoading(true);

        try {
            const response = await fetch('/StudentAIChat/Send', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': antiForgeryToken
                },
                body: JSON.stringify({
                    sessionId: parseInt(sessionId, 10),
                    content: content
                })
            });

            const data = await response.json();

            if (response.ok && data.success) {
                appendMessage('ASSISTANT', data.message.content);
            } else {
                appendMessage('ASSISTANT', data.error || 'Sorry, an error occurred. Please try again.');
            }
        } catch (error) {
            console.error('Send message error:', error);
            appendMessage('ASSISTANT', 'Unable to send message. Please check your network connection.');
        } finally {
            setLoading(false);
        }
    }

    function handleKeyDown(e) {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            chatForm.dispatchEvent(new Event('submit'));
        }
    }

    function autoResize() {
        messageInput.style.height = 'auto';
        messageInput.style.height = Math.min(messageInput.scrollHeight, 120) + 'px';
    }

    function appendMessage(senderType, content) {
        const welcomeMessage = messagesList.querySelector('.welcome-message');
        if (welcomeMessage) {
            welcomeMessage.remove();
        }

        const isUser = senderType === 'USER';
        const now = new Date();
        const timeStr = now.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });

        const messageDiv = document.createElement('div');
        messageDiv.className = `message ${isUser ? 'user' : 'assistant'}`;
        messageDiv.innerHTML = `
            <div class="message-avatar">
                <i class="fas fa-${isUser ? 'user' : 'robot'}"></i>
            </div>
            <div class="message-bubble">
                <div class="message-content">${escapeHtml(content)}</div>
                <div class="message-time">${timeStr}</div>
            </div>
        `;

        messagesList.appendChild(messageDiv);
        scrollToBottom();
    }

    function setLoading(loading) {
        sendBtn.disabled = loading;
        messageInput.disabled = loading;
        typingIndicator.style.display = loading ? 'flex' : 'none';

        if (loading) {
            scrollToBottom();
        }
    }

    function scrollToBottom() {
        requestAnimationFrame(() => {
            messagesContainer.scrollTop = messagesContainer.scrollHeight;
        });
    }

    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }
})();
