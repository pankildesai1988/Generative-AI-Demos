FROM node:22-alpine AS build
WORKDIR /app
COPY frontend/shared ./frontend/shared
COPY frontend/finance-demo ./frontend/finance-demo
COPY package.json ./
RUN npm install --workspace=@arnir/finance-demo --workspace=@arnir/shared
RUN npm run build --workspace=@arnir/finance-demo

FROM nginx:1.27-alpine
COPY --from=build /app/frontend/finance-demo/dist /usr/share/nginx/html
COPY frontend/finance-demo/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
