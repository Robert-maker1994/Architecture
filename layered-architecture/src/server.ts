import express from "express";
import cors from "cors";
import apiRoutes from './api/routes';

const app = express();
const port = process.env.PORT || 3001; 

app.use(cors({
    origin:"*"
})); 
app.use(express.json()); 

app.use('/', apiRoutes);

app.listen(port, () => {
  console.log(`Server is running on port ${port}`);
});

export default app;