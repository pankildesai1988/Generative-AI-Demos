FROM node:22-alpine AS build
WORKDIR /app
COPY frontend/shared ./frontend/shared
COPY frontend/healthcare-demo ./frontend/healthcare-demo
COPY package.json ./
RUN npm install --legacy-peer-deps --workspace=@arnir/healthcare-demo --workspace=@arnir/shared
RUN npm run build --workspace=@arnir/healthcare-demo

FROM nginx:1.27-alpine
COPY --from=build /app/frontend/healthcare-demo/dist /usr/share/nginx/html
COPY frontend/healthcare-demo/nginx.conf /etc/nginx/conf.d/default.conf
COPY frontend/healthcare-demo/entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh
EXPOSE 80
HEALTHCHECK CMD wget -q --spider http://localhost:80/ || exit 1
ENTRYPOINT ["/entrypoint.sh"]
