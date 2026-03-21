FROM node:22-alpine AS build
WORKDIR /app
COPY frontend/shared ./frontend/shared
COPY frontend/ecommerce-demo ./frontend/ecommerce-demo
COPY package.json ./
RUN npm install --legacy-peer-deps --workspace=@arnir/ecommerce-demo --workspace=@arnir/shared
RUN npm run build --workspace=@arnir/ecommerce-demo

FROM nginx:1.27-alpine
COPY --from=build /app/frontend/ecommerce-demo/dist /usr/share/nginx/html
COPY frontend/ecommerce-demo/nginx.conf /etc/nginx/conf.d/default.conf
COPY frontend/ecommerce-demo/entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh
EXPOSE 80
HEALTHCHECK CMD wget -q --spider http://localhost:80/ || exit 1
ENTRYPOINT ["/entrypoint.sh"]
