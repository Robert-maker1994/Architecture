import { Router } from 'express';
import { DomainValidation } from '../domain/todo.logic';
import { InMemoryTodoRepository } from '../infrastructure/data-access/todo.repo';
import { TodoApplicationService } from '../application/service/todo.service';
import { TodoController } from './controller/todoController';

const router = Router();

// Setup dependencies (Dependency Injection)
// In a real app, you'd use a DI container or a more sophisticated setup
const domainValidation = new DomainValidation();
const todoRepository = new InMemoryTodoRepository();
const todoAppService = new TodoApplicationService(todoRepository,domainValidation);
const todoController = new TodoController(todoAppService);

// Define routes
router.post('/todos', (req: any, res: any) => todoController.createTodo(req, res));
router.get('/todos', (req: any, res: any) => todoController.getTodos(req, res));
router.get('/todos/:id', (req: any, res: any) => todoController.getTodoById(req, res));
router.put('/todos/:id', (req: any, res: any) => todoController.updateTodo(req, res));
router.patch('/todos/:id/toggle', (req: any, res: any) => todoController.toggleTodo(req, res));
router.delete('/todos/:id', (req: any, res: any) => todoController.deleteTodo(req, res));

export default router;
