# Dev-oriented image matching docker-compose (Vite dev server on 5173).
# VITE_API_URL is supplied at runtime by compose and read by Vite on startup.
FROM node:24-alpine
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .

EXPOSE 5173
CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0"]
