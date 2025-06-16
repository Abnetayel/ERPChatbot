import React, { useState, useRef, useEffect } from 'react';
import './ChatWindow.css';
import chatService from '../services/chatService';

// Loading dots animation component
const LoadingDots = () => (
  <div className="flex space-x-1">
    <div className="w-2 h-2 bg-gray-400 rounded-full animate-bounce" style={{ animationDelay: '0ms' }}></div>
    <div className="w-2 h-2 bg-gray-400 rounded-full animate-bounce" style={{ animationDelay: '150ms' }}></div>
    <div className="w-2 h-2 bg-gray-400 rounded-full animate-bounce" style={{ animationDelay: '300ms' }}></div>
  </div>
);

// Typing indicator component
const TypingIndicator = () => (
  <div className="flex items-center space-x-2 text-sm text-gray-500">
    <span>Bot is typing</span>
    <LoadingDots />
  </div>
);

// Format timestamp
const formatTimestamp = (timestamp) => {
  const date = new Date(timestamp);
  return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
};

const ChatWindow = () => {
    const [messages, setMessages] = useState([]);
    const [inputMessage, setInputMessage] = useState('');
    const [isLoading, setIsLoading] = useState(false);
    const messagesEndRef = useRef(null);
    const [sessionId] = useState(() => {
        // Generate a new session ID when component mounts
        return Math.random().toString(36).substring(2, 15);
    });

    const scrollToBottom = () => {
        messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
    };

    useEffect(() => {
        scrollToBottom();
    }, [messages]);

    const handleSendMessage = async (e) => {
        e.preventDefault();
        if (!inputMessage.trim()) return;

        const userMessage = inputMessage.trim();
        setInputMessage('');
        
        // Add user message to chat
        setMessages(prev => [...prev, { role: 'user', content: userMessage }]);
        
        setIsLoading(true);
        try {
            const response = await chatService.sendMessage(userMessage, sessionId);
            
            // Add main response
            setMessages(prev => [...prev, { 
                role: 'assistant', 
                content: response.mainResponse,
                type: 'main'
            }]);

            // Add follow-up question if exists
            if (response.followUpQuestion) {
                setMessages(prev => [...prev, { 
                    role: 'assistant', 
                    content: response.followUpQuestion,
                    type: 'follow-up'
                }]);
            }
        } catch (error) {
            console.error('Error sending message:', error);
            setMessages(prev => [...prev, { 
                role: 'assistant', 
                content: 'Sorry, I encountered an error. Please try again.',
                type: 'error'
            }]);
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="chat-window">
            <div className="chat-messages">
                {messages.map((message, index) => (
                    <div 
                        key={index} 
                        className={`message ${message.role === 'user' ? 'user-message' : 'bot-message'} ${message.type || ''}`}
                    >
                        <div className="message-content">
                            {message.content}
                        </div>
                    </div>
                ))}
                {isLoading && (
                    <div className="message bot-message">
                        <div className="message-content">
                            <div className="typing-indicator">
                                <span></span>
                                <span></span>
                                <span></span>
                            </div>
                        </div>
                    </div>
                )}
                <div ref={messagesEndRef} />
            </div>
            <form onSubmit={handleSendMessage} className="chat-input-form">
                <input
                    type="text"
                    value={inputMessage}
                    onChange={(e) => setInputMessage(e.target.value)}
                    placeholder="Type your message..."
                    disabled={isLoading}
                />
                <button type="submit" disabled={isLoading || !inputMessage.trim()}>
                    Send
                </button>
            </form>
        </div>
    );
};

export default ChatWindow; 