# Сеть

```Mermaid
flowchart LR
	subgraph Не авторитарная архитектура		
		direction LR
		subgraph Server Side
			MainServer_n("Сервер")
		end
		subgraph World Side
			
			Client1_n("Клиент\nи\nХост")
			Client2_n("Клиент")
		end	
	end	
	subgraph Авторитарная архитектура		
		direction LR
		subgraph Server Side
			MainServer("Сервер")
			Host("Хост")
		end
		subgraph World Side
			
			Client1("Клиент")
			Client2("Клиент")
		end	
	end		
	
	Client1 <----> MainServer		
	Client2 <----> MainServer
	MainServer <----> Host	
	Client1_n <----> MainServer_n		
	Client2_n<----> MainServer_n
	
	linkStyle default stroke:green
```

# Сервер

```Mermaid
flowchart LR	
	subgraph Server	
		input(("Input\nGate"))
		subgraph Room_1
			client_1("Client")
			client_2("Client")
			host_1("Host")
			update1["network-update"]
		end
		subgraph Room_2
			update2["network-update"]
			client_9("Client")
			client_10("Client")
			host_2("Host")
		end
	end			
	
	input <---> Room_1
	input <---> Room_2
	update1 ----> update1
	update2 ----> update2
	
	linkStyle default stroke:green
```