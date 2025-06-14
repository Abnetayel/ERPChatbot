import axios from 'axios';

const API_URL = 'http://localhost:7001/api';

export const createSession = async () => {
    try {
        const response = await fetch(`${API_URL}/chat/sessions`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            }
        });

        if (!response.ok) {
            throw new Error('Failed to create chat session');
        }

        return await response.json();
    } catch (error) {
        console.error('Error creating chat session:', error);
        throw error;
    }
};

export const sendMessage = async (sessionId, content) => {
    try {
        const response = await fetch(`${API_URL}/chat/messages`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                sessionId,
                content,
                isUserMessage: true
            }),
        });

        if (!response.ok) {
            throw new Error('Failed to send message');
        }

        return await response.json();
    } catch (error) {
        console.error('Error sending message:', error);
        throw error;
    }
};

export const getSessionMessages = async (sessionId) => {
    try {
        const response = await fetch(`${API_URL}/chat/sessions/${sessionId}/messages`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            },
        });

        if (!response.ok) {
            throw new Error('Failed to fetch messages');
        }

        return await response.json();
    } catch (error) {
        console.error('Error fetching messages:', error);
        throw error;
    }
}; 