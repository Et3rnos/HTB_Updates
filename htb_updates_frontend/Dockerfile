FROM node:alpine AS builder
WORKDIR /app

COPY htb_updates_frontend/package.json package.json
RUN npm install

COPY htb_updates_frontend .

RUN npm run build

FROM nginx:alpine
WORKDIR /usr/share/nginx/html

RUN rm -rf *

COPY --from=builder /app/build .

COPY htb_updates_frontend/nginx.conf /etc/nginx/conf.d/default.conf

ENTRYPOINT ["nginx", "-g", "daemon off;"]