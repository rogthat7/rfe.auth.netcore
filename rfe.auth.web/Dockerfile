# Build stage
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build

# Production stage
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
# Copy standard nginx configuration to support routing
RUN echo 'server { \
    listen 80; \
    server_name localhost; \
    return 301 https://localhost:3001$request_uri; \
} \
server { \
    listen 443 ssl; \
    server_name localhost; \
    ssl_certificate /certs/localhost+2.pem; \
    ssl_certificate_key /certs/localhost+2-key.pem; \
    location / { \
        root /usr/share/nginx/html; \
        index index.html index.htm; \
        try_files $uri $uri/ /index.html; \
    } \
    location /api { \
        proxy_pass https://rfe-auth-api:5001; \
        proxy_set_header Host $http_host; \
        proxy_set_header X-Real-IP $remote_addr; \
        proxy_ssl_verify off; \
    } \
    location /connect { \
        proxy_pass https://rfe-auth-api:5001; \
        proxy_set_header Host $http_host; \
        proxy_set_header X-Real-IP $remote_addr; \
        proxy_ssl_verify off; \
    } \
    location /signin-github { \
        proxy_pass https://rfe-auth-api:5001; \
        proxy_set_header Host $http_host; \
        proxy_set_header X-Real-IP $remote_addr; \
        proxy_ssl_verify off; \
    } \
}' > /etc/nginx/conf.d/default.conf
EXPOSE 443
CMD ["nginx", "-g", "daemon off;"]
