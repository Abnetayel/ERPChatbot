import React, { useState, useRef, useEffect } from 'react';
import './ChatWindow.css';
import chatService from '../services/chatService';
import logo from '../assets/logo.jpg';

const BOT_NAME = 'ERP Chatbot Assistant';
const USER_AVATAR = '🧑';

const LoadingDots = () => (
  <div className="loading-dots">
    <span></span>
    <span></span>
    <span></span>
  </div>
);

const TypingIndicator = () => (
  <div className="message-row bot-row animate-message">
    <div className="avatar bot-avatar"><img src={logo} alt="Bot" /></div>
    <div className="message-bubble bot typing-indicator-bubble">
      <span>Bot is typing</span> <LoadingDots />
    </div>
  </div>
);

const formatTimestamp = (timestamp) => {
  const date = new Date(timestamp);
  return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
};

const ChatWindow = () => {
  const [messages, setMessages] = useState([]);
  const [inputMessage, setInputMessage] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [errorIndex, setErrorIndex] = useState(null); // index of the error bot message
  const [lastUserMessage, setLastUserMessage] = useState(null);
  const messagesEndRef = useRef(null);
  const [sessionId] = useState(() => Math.random().toString(36).substring(2, 15));

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  const handleSendMessage = async (e, retry = false, overrideMessage = null) => {
    if (e) e.preventDefault();
    const userMessage = overrideMessage !== null
      ? overrideMessage
      : (retry ? lastUserMessage : inputMessage.trim());
    if (!userMessage) return;
    if (!retry) setInputMessage('');
    setIsLoading(true);
    setErrorIndex(null);
    if (!retry) {
      setLastUserMessage(userMessage);
      setMessages(prev => [
        ...prev,
        { role: 'user', content: userMessage, timestamp: new Date().toISOString() }
      ]);
    }
    try {
      const response = await chatService.sendMessage(userMessage, sessionId);
      let combinedContent = response.mainResponse;
      if (response.followUpQuestion) {
        combinedContent += "\n\n" + response.followUpQuestion;
      }
      setMessages(prev => {
        // On retry, replace the error and failed bot message
        if (retry && errorIndex !== null) {
          const newMessages = prev.slice(0, errorIndex);
          newMessages.push({
            role: 'assistant',
            content: combinedContent,
            timestamp: new Date().toISOString()
          });
          return newMessages;
        }
        // Normal add
        return [
          ...prev,
          {
            role: 'assistant',
            content: combinedContent,
            timestamp: new Date().toISOString()
          }
        ];
      });
    } catch (error) {
      setMessages(prev => {
        // On retry, replace the error and failed bot message with new error
        if (retry && errorIndex !== null) {
          const newMessages = prev.slice(0, errorIndex);
          newMessages.push({
            role: 'assistant',
            content: 'Sorry, I encountered an error. Please try again.',
            type: 'error',
            timestamp: new Date().toISOString()
          });
          return newMessages;
        }
        // Normal add
        return [
          ...prev,
          {
            role: 'assistant',
            content: 'Sorry, I encountered an error. Please try again.',
            type: 'error',
            timestamp: new Date().toISOString()
          }
        ];
      });
      // Set error index to the last message (the error bot message)
      setErrorIndex(messages.length + (retry ? 0 : 1));
    } finally {
      setIsLoading(false);
    }
  };

  const handleRetry = () => {
    if (errorIndex !== null && lastUserMessage) {
      // Remove the error message from messages
      setMessages(prev => prev.slice(0, errorIndex));
      setErrorIndex(null);
      // Re-send the last user message
      handleSendMessage(null, true);
    }
  };

  const handleRefresh = () => {
    window.location.reload();
  };

  return (
    <div className="drift-chatbot-container">
      <div className="drift-chatbot-window">
        {/* Header */}
        <div className="drift-chatbot-header" style={{ position: 'relative' }}>
          <img src={logo} alt="Bot Logo" className="drift-bot-logo" />
          <div className="drift-bot-header-info">
            <div className="drift-bot-name">{BOT_NAME}</div>
            <div className="drift-bot-status"><span className="drift-online-dot" /> Online Now</div>
          </div>
          <button
            className="refresh-btn"
            onClick={handleRefresh}
            title="Refresh Chat"
            style={{ position: 'absolute', right: 16, top: 16 }}
          >
            &#x21bb;
          </button>
        </div>
        {/* Messages */}
        <div className="drift-chatbot-messages">
          {messages.length === 0 && !isLoading && (
            <div className="drift-message-row bot-row animate-message">
              <div className="avatar bot-avatar"><img src={logo} alt="Bot" /></div>
              <div className="drift-message-bubble bot">
                Hello! I am your ERP Chatbot Assistant. How can I help you today?
              </div>
            </div>
          )}
          {messages.map((message, index) => (
            message.role === 'assistant' ? (
              <React.Fragment key={index}>
                <div className="drift-message-row bot-row animate-message">
                  <div className="avatar bot-avatar"><img src={logo} alt="Bot" /></div>
                  <div className={`drift-message-bubble bot${message.type === 'error' ? ' error' : ''}`}>
                    {message.content}
                    <div className="drift-message-timestamp">{formatTimestamp(message.timestamp)}</div>
                  </div>
                </div>
                {message.type === 'error' && errorIndex === index && (
                  <div className="chat-error-notification">
                    <span>Sorry, I encountered an error. </span>
                    <button onClick={handleRetry} className="chat-error-retry">Refresh &amp; Send Again</button>
                  </div>
                )}
              </React.Fragment>
            ) : (
              <div
                key={message.id || index}
                className={`drift-message-row user-row animate-message`}
              >
                <div className="avatar user-avatar"><span>{USER_AVATAR}</span></div>
                <div className="drift-message-bubble user">
                  <div className="message-row">
                    <span>{message.content}</span>
                  </div>
                  {/* {message.edited && <span className="edited-label">(edited)</span>} */}
                  <div className="drift-message-timestamp">{formatTimestamp(message.timestamp)}</div>
                </div>
              </div>
            )
          ))}
          {isLoading && <TypingIndicator />}
          <div ref={messagesEndRef} />
        </div>
        {/* Input */}
        <form onSubmit={handleSendMessage} className="drift-chatbot-input-form">
          <input
            type="text"
            value={inputMessage}
            onChange={(e) => setInputMessage(e.target.value)}
            placeholder={`Reply to ${BOT_NAME}...`}
            disabled={isLoading}
          />
          <button type="submit" disabled={isLoading || !inputMessage.trim()} className="send-btn">
            Send
          </button>
        </form>
      </div>
    </div>
  );
};

export default ChatWindow; 