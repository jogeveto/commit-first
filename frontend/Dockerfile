# Frontend React (Vite + TS). Dev server dentro del contenedor (host sin toolchain de proyecto).
# Scaffold: imagen de desarrollo. La imagen de producción (build estático + nginx) se define
# en el slice de despliegue.
FROM node:20-alpine AS dev
WORKDIR /app

COPY package.json ./
RUN npm install

COPY . ./

EXPOSE 5173
CMD ["npm", "run", "dev"]
