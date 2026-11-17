# XIIGen FAQ

These are the ten product questions from XIIGen, adapted to the shared .NET platform and its companion .NET/Angular flows. Start with the [README](../README.md) for the repository map.

## 1. Does the agent actually check its own work, or just say it did?

XIIGen treats generation and verification as separate responsibilities. Generated output is subject to validation, scoring and review; an author's statement that a task is complete is not the acceptance decision. The developer remains responsible for deciding whether the result meets the requested contract.

In this split, flow-level generation and arbitration belong to the [flows repository](https://github.com/lubochka/xiigen-flows-dot-net-angular). This platform supplies common result and context contracts, while its [test sources](../tests) and [review skills](skills) describe checks on those contracts. A source file or an old report should not be confused with the result of a particular execution.

## 2. Is this a hybrid architecture, or a wrapper around one model?

XIIGen combines model-assisted reasoning with deterministic application behavior, retrieval and infrastructure services. Its fabric-first architecture separates a capability's contract from its implementation, so business behavior need not be tied to one provider SDK.

In .NET, the [fabric contracts](../src/Xiigen.Kernel/FabricContracts.cs) include `IDocumentStore`, `IIdempotencyStore` and `IOutboxStore`. The [in-memory implementations](../src/Xiigen.Fabrics.InMemory) illustrate that separation. Model and retrieval workflows belong to the flow layer; the platform is their common foundation, not a model wrapper.

## 3. Can the AI invent the algorithm, or does it only type the code?

The product separates intent, planning, decomposition, generation and review. An agent can propose an approach and help turn it into executable application behavior. The developer defines the goal and constraints; the proposal is reviewed against those constraints rather than accepted because it sounds plausible.

The companion flows include a meta-flow engine, a schema registry and DAG workflow, and arbitration. Here, [MicroserviceBase](../src/Xiigen.Kernel/MicroserviceBase.cs) and the kernel contracts provide the stable service structure those flows use. Creative planning does not replace explicit contracts or reproducible business rules.

## 4. Does this engine train models, or use new training methods?

The engine consumes models; it is not itself a model-weight training system. XIIGen's learning loop concerns feedback, preference signals and retrieval memory. Model-weight training belongs to the separate XIIGen model core.

In the .NET implementation, RAG feedback and agent workflows live in the flows repository. The platform supplies shared infrastructure. Improving retrieved knowledge, recording a review outcome and training model weights are different operations; the word “learning” does not make them interchangeable.

## 5. What does “agentic” mean here, concretely?

An agent works toward a stated intent through a sequence of bounded actions: planning, retrieving context, proposing or generating changes, and obtaining review or a human decision. The work has an inspectable result and belongs to an explicit tenant and execution context.

The platform represents that context in [TenantContext](../src/Xiigen.Kernel/TenantContext.cs) and carries it through [MicroserviceBase](../src/Xiigen.Kernel/MicroserviceBase.cs). The flows repository supplies the platform-agent and human-interaction workflows. Agentic execution does not transfer product ownership away from the human.

## 6. How does this change the developer's role?

The developer owns the intent, architecture, constraints and acceptance decisions. Agents can perform bounded implementation and analysis work, but choosing a product requirement is different from executing it.

The [skill usage guide](HOW-TO-USE-SKILLS.md) and [skills](skills) keep the method in repository files. For a cross-repository change, the developer also owns the boundary: shared infrastructure belongs here, while domain behavior belongs in the flows repository. Changes to package contracts need to be coordinated with their consumers.

## 7. Which coding agent does this bet on — Claude, Codex or Cursor?

The product's durable context is the repository: source, contracts, documentation and skills. Claude, Codex, Cursor and other compatible tools can work with that context. The project does not need to turn one coding assistant into the architecture.

Use the [repository skills](skills) with the host's supported loading mechanism and the current task instructions. A coding assistant used to develop XIIGen is also distinct from a model provider invoked by a running application.

## 8. Is this a real system, or a demo?

There is concrete source to explore: the [kernel](../src/Xiigen.Kernel), [fabric implementations](../src/Xiigen.Fabrics.InMemory), [marketplace](../src/Xiigen.Platform.Marketplace), [platform API](../src/Xiigen.Platform.Api) and [test projects](../tests). The companion repository contains the business microservices and Angular clients.

Start at those components and follow the contracts into the flow that interests you. This repository is the shared platform portion of the system; the two repositories together explain how the product is assembled.

## 9. Isn't this just vibe coding with extra steps?

The extra structure has a purpose: make intent, contracts, generated output and review explicit. XIIGen uses shared service conventions, tenant context, structured results and separate review responsibilities so that a plausible answer can be examined against a concrete requirement.

For example, [MicroserviceBase](../src/Xiigen.Kernel/MicroserviceBase.cs) exposes tenant and correlation context, and the [fabric contracts](../src/Xiigen.Kernel/FabricContracts.cs) describe persistence and outbox behavior. These are concrete architectural boundaries, not instructions to accept whatever a model writes.

## 10. How does this relate to local LLMs, agent frameworks and AI gateways?

These are complementary layers. A model supplies reasoning or generation; an agent framework coordinates actions; retrieval supplies context; a gateway can route provider requests. XIIGen organizes these capabilities around application flows and shared contracts.

The .NET platform supplies infrastructure boundaries, while the flows repository contains orchestration, adaptive RAG and agent workflows. A local model can be an integration choice at the appropriate provider boundary; the XIIGen model core is separate from this engine. Choosing a model does not replace the application's tenant, result or business contracts.

[Back to the platform README](../README.md)
