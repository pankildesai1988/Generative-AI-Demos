FROM node:22-alpine AS build
WORKDIR /app
COPY frontend/shared ./frontend/shared
COPY frontend/healthcare-demo ./frontend/healthcare-demo
COPY package.json ./
RUN npm install --workspace=@arnir/healthcare-demo --workspace=@arnir/shared
RUN npm run build --workspace=@arnir/healthcare-demo

FROM nginx:1.27-alpine
COPY --from=build /app/frontend/healthcare-demo/dist /usr/share/nginx/html
COPY frontend/healthcare-demo/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
