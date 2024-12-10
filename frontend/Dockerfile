# Frontend Dockerfile using Next.js

FROM node:20-alpine AS build
WORKDIR /app

# Install dependencies & packages
COPY package*.json ./
RUN npm install

# Copy the rest of the frontend code
COPY . .

# Build the application
RUN npm run build

# Production stage
FROM node:20-alpine AS production
WORKDIR /app

# Copy built assets from build stage to minimize the size of the container
COPY --from=build /app/package*.json ./
COPY --from=build /app/next.config.mjs ./
COPY --from=build /app/public ./public
COPY --from=build /app/.next ./.next

# Install production dependencies
RUN npm install next

# Expose the port the app runs on
EXPOSE 3000
ENV NEXT_PUBLIC_FRONTEND_URL="https://localhost:3000"
ENV NEXT_PUBLIC_BACKEND_URL="https://localhost:5000"

# Start the application
CMD ["npx", "next", "start"]