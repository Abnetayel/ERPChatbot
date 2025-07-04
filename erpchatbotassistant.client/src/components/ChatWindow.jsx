import React, { useState, useRef, useEffect } from 'react';
import './ChatWindow.css';
import chatService from '../services/chatService';
import logo from '../assets/logo.jpg';
import ReactMarkdown from 'react-markdown';
import { FiCopy, FiCheck } from 'react-icons/fi';

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

// Save messages to localStorage by sessionId
const saveSessionToStorage = (sessionId, messages) => {
  localStorage.setItem(`chat_session_${sessionId}`, JSON.stringify({
    messages,
    updated: new Date().toISOString()
  }));
};

// Get all sessions from localStorage
const getAllSessionsFromStorage = () => {
  return Object.keys(localStorage)
    .filter(key => key.startsWith('chat_session_'))
    .map(key => {
      const { messages, updated } = JSON.parse(localStorage.getItem(key));
      return {
        sessionId: key.replace('chat_session_', ''),
        messages,
        updated
      };
    })
    .sort((a, b) => new Date(b.updated) - new Date(a.updated));
};

const ChatWindow = () => {
  const [messages, setMessages] = useState([]);
  const [inputMessage, setInputMessage] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [errorIndex, setErrorIndex] = useState(null); // index of the error bot message
  const [lastUserMessage, setLastUserMessage] = useState(null);
  const messagesEndRef = useRef(null);
  const [sessionId, setSessionId] = useState(() => Math.random().toString(36).substring(2, 15));
  const [isVisible, setIsVisible] = useState(false); // default to hidden
  const [showMenu, setShowMenu] = useState(false);
  const menuRef = useRef();
  const [showRecentChats, setShowRecentChats] = useState(false);
  const [recentSessions, setRecentSessions] = useState([]);
  const [copiedIndex, setCopiedIndex] = useState(null);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  useEffect(() => {
    const handleClickOutside = (event) => {
      if (menuRef.current && !menuRef.current.contains(event.target)) {
        setShowMenu(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  useEffect(() => {
    if (messages.length > 0) {
      saveSessionToStorage(sessionId, messages);
    }
  }, [messages, sessionId]);

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
        combinedContent += "<br/>" + response.followUpQuestion;
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
    // window.location.reload();
    setMessages([]);
    setInputMessage('');
    setErrorIndex(null);
    setLastUserMessage(null);
    setIsVisible(true);
    setShowMenu(false);
  };

  const handleStartNewChat = () => {
    setSessionId(Math.random().toString(36).substring(2, 15));
    setMessages([]);
    setInputMessage('');
    setErrorIndex(null);
    setLastUserMessage(null);
    setShowMenu(false);
  };

  const handleViewRecentChats = () => {
    setRecentSessions(getAllSessionsFromStorage());
    setShowRecentChats(true);
    setShowMenu(false);
  };

  const handleOpenSession = (session) => {
    setSessionId(session.sessionId);
    setMessages(session.messages);
    setShowRecentChats(false);
    setShowMenu(false);
  };

  return (
    <>
      {/* Open chat button (💬) when chat is closed */}
      {!isVisible && (
        <button
          className="open-chat-btn"
          aria-label="Open chat"
          onClick={() => setIsVisible(true)}
        >
          💬
        </button>
      )}
      {/* Chat window and close button when chat is open */}
      {isVisible && (
        <div className="drift-chatbot-container" style={{ position: 'fixed', bottom: '90px', right: '32px', zIndex: 1000 }}>
          <div className="drift-chatbot-window">
            {/* Header */}
            <div className="drift-chatbot-header" style={{ position: 'relative' }}>
              <img src={logo} alt="Bot Logo" className="drift-bot-logo" />
              <div className="drift-bot-header-info">
                <div className="drift-bot-name">{BOT_NAME}</div>
                <div className="drift-bot-status"><span className="drift-online-dot" /> Online Now</div>
              </div>
              <div className="header-actions">           
                <button
                  className="menu-btn"
                  onClick={() => setShowMenu((v) => !v)}
                  aria-label="More options"
                >
                  &#x22EE;
                </button>
                {showMenu && (
                  <div className="chat-menu-dropdown" ref={menuRef}>
                    <button onClick={handleStartNewChat}>Start a new chat</button>
                    <button onClick={handleRefresh}>refresh</button>
                    <button onClick={handleViewRecentChats}>View recent chats</button>
                  </div>
                )}
              </div>
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
                        <ReactMarkdown>
                          {typeof message.content === 'string'
                            ? message.content.replace(/<br\s*\/?>/gi, '\n')
                            : message.content}
                        </ReactMarkdown>
                        <div className="drift-message-timestamp">{formatTimestamp(message.timestamp)}</div>
                      </div>
                      <button
                        className="copy-btn"
                        onClick={() => {
                          navigator.clipboard.writeText(
                            typeof message.content === 'string'
                              ? message.content.replace(/<br\s*\/?>/gi, '\n')
                              : ''
                          );
                          setCopiedIndex(index);
                          setTimeout(() => setCopiedIndex(null), 1200);
                        }}
                        title="Copy"
                      >
                        {copiedIndex === index ? <FiCheck size={22} /> : <FiCopy size={22} />}
                      </button>
                      {/* {copiedIndex === index && (
                        <span className="copied-feedback">Copied!</span>
                      )} */}
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
          {/* Show close button only if recent chats panel is not open */}
          {!showRecentChats && (
            <button
              className="close-chat-btn"
              aria-label="Close chat"
              onClick={() => setIsVisible(false)}
            >
              &#x2B9F;
            </button>
          )}
          {/* Recent chats panel overlays the chat window if open */}
          {showRecentChats && (
            <div className="recent-chats-panel">
              <div className="recent-chats-header">
                <button className="back-btn" onClick={() => setShowRecentChats(false)}>&larr;</button>
                <span>Recent chats</span>
              </div>
              <ul className="recent-chats-list">
                {recentSessions.length === 0 ? (
                  <li className="recent-chat-empty">No recent chats found.</li>
                ) : recentSessions.map(session => (
                  <li
                    key={session.sessionId}
                    className="recent-chat-row"
                    onClick={() => handleOpenSession(session)}
                  >
                    <div className="recent-chat-avatar">EC</div>
                    <div className="recent-chat-info">
                      <div className="recent-chat-title">
                        {session.messages[0]?.content?.slice(0, 32) || 'No messages'}
                      </div>
                      <div className="recent-chat-subtitle">
                        ERP Chatbot Assistant &bull; {new Date(session.updated).toLocaleString()}
                      </div>
                    </div>
                    <span className="recent-chat-status">Open</span>
                    <span className="recent-chat-arrow">&#x203A;</span>
                  </li>
                ))}
              </ul>
            </div>
          )}
        </div>
      )}
    </>
  );
};

export default ChatWindow; 