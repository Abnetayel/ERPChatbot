import axios from 'axios';

// Make sure this matches your backend server URL
const API_URL = import.meta.env.VITE_API_URL || 'https://localhost:5001/api';

const handleResponse = async (response) => {
    console.log('Response status:', response.status);
    const data = await response.json();
    console.log('Response data:', data);
    
    if (!response.ok) {
        console.error('Error response:', data);
        // If the response has an error message, use it
        if (data && data.error) {
            throw new Error(data.error);
        }
        // Otherwise, use a generic error message
        throw new Error(`Server error: ${response.status} ${response.statusText}`);
    }

    return data;
};

const chatService = {
    async sendMessage(message, sessionId) {
        try {
            if (!message) {
                throw new Error('Message cannot be empty');
            }

            if (!sessionId) {
                throw new Error('Session ID is required');
            }

            console.log('Sending request to:', `${API_URL}/chat`);
            console.log('Request payload:', { message, sessionId });

            const response = await axios.post(`${API_URL}/chat`, {
                message,
                sessionId
            }, {
                headers: {
                    'Content-Type': 'application/json'
                },
                timeout: 30000 // 30 second timeout
            });

            console.log('Response received:', response.data);

            if (!response.data) {
                throw new Error('No response data received');
            }

            return response.data;
        } catch (error) {
            console.error('Error in sendMessage:', error);
            
            if (error.response) {
                // The request was made and the server responded with a status code
                // that falls out of the range of 2xx
                console.error('Server response error:', error.response.data);
                throw new Error(error.response.data?.message || `Server error: ${error.response.status}`);
            } else if (error.request) {
                // The request was made but no response was received
                console.error('No response received:', error.request);
                throw new Error('No response received from server. Please check your internet connection and ensure the server is running.');
            } else {
                // Something happened in setting up the request that triggered an Error
                console.error('Request setup error:', error.message);
                throw new Error(error.message || 'An error occurred while sending your message');
            }
        }
    },

    // async getRecentMessages() {
    //     try {
    //         console.log('Fetching recent messages');
    //         const response = await fetch(`${API_URL}/chat/messages`, {
    //             method: 'GET',
    //             headers: {
    //                 'Content-Type': 'application/json',
    //             },
    //         });

    //         if (!response.ok) {
    //             const errorData = await response.json();
    //             console.error('Failed to fetch messages:', errorData);
    //             throw new Error(errorData.error || 'Failed to fetch messages');
    //         }

    //         const data = await response.json();
    //         console.log('Messages fetched successfully:', data);
    //         return data;
    //     } catch (error) {
    //         console.error('Error fetching messages:', error);
    //         throw new Error('Failed to fetch messages. Please try again.');
    //     }
    // }
};

export default chatService; 
