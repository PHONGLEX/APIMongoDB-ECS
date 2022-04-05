from aws_cdk import (
    aws_autoscaling as autoscaling,
    aws_ec2 as ec2,
    aws_elasticloadbalancingv2 as elbv2,
    aws_ecs as ecs,
    App, CfnOutput, Duration, Stack,
    aws_iam as iam,
    aws_ecr as ecr
)

app = App()
stack = Stack(app, "sample-aws-ec2-integ-ecs")

ecr_repository = ecr.Repository(stack,
                                        "ecs-devops-sandbox-repository",
                                        repository_name="ecs-devops-sandbox-repository")

# Create a cluster
vpc = ec2.Vpc(
    stack, "ecs-devops-sandbox-vpc",
    max_azs=2
)

cluster = ecs.Cluster(
    stack, "ecs-devops-sandbox-cluster",
    cluster_name="ecs-devops-sandbox-cluster",
    vpc=vpc
)

asg = autoscaling.AutoScalingGroup(
    stack, "DefaultAutoScalingGroup",
    instance_type=ec2.InstanceType.of(
                         ec2.InstanceClass.BURSTABLE2,
                         ec2.InstanceSize.MICRO),
    machine_image=ecs.EcsOptimizedImage.amazon_linux2(),
    vpc=vpc,
)
capacity_provider = ecs.AsgCapacityProvider(stack, "AsgCapacityProvider",
    auto_scaling_group=asg
)
cluster.add_asg_capacity_provider(capacity_provider)

task_role = iam.Role(stack, "ecs-devops-sandbox-task-role",
                            assumed_by=iam.ServicePrincipal("ecs-tasks.amazonaws.com"),
                            role_name="ecs-devops-sandbox-task-role")

task_role.add_to_policy(iam.PolicyStatement(
    effect=iam.Effect.ALLOW,
    resources=["arn:aws:ssm:us-east-1:134477770615:parameter/test/weather-forecast/*"],
    actions=[
        "ssm:PutParameter",
        "ssm:DeleteParameter",
        "ssm:GetParameterHistory",
        "ssm:GetParametersByPath",
        "ssm:GetParameters",
        "ssm:GetParameter",
        "ssm:DeleteParameters"          
    ]
))

task_role.add_to_policy(iam.PolicyStatement(
    effect=iam.Effect.ALLOW,
    resources=["arn:aws:logs:*:*:*"],
    actions=[
        "logs:CreateLogGroup",
        "logs:CreateLogStream",
        "logs:PutLogEvents",
        "logs:DescribeLogStreams"         
    ]
))

# Create Task Definition
task_definition = ecs.Ec2TaskDefinition(stack, "ecs-devops-sandbox-task-definition", task_role=task_role, family="ecs-devops-sandbox-task-definition")

container = task_definition.add_container(
    "ecs-devops-sandbox",
    image=ecs.ContainerImage.from_registry("amazon/amazon-ecs-sample"),
    memory_limit_mib=256
)

port_mapping = ecs.PortMapping(
    container_port=80,
    host_port=80,
    protocol=ecs.Protocol.TCP
)
container.add_port_mappings(port_mapping)

# Create Service
service = ecs.Ec2Service(
    stack, "ecs-devops-sandbox-service",
    cluster=cluster,
    task_definition=task_definition,
    service_name="ecs-devops-sandbox-service"
)

# Create ALB
lb = elbv2.ApplicationLoadBalancer(
    stack, "LB",
    vpc=vpc,
    internet_facing=True
)

listener = lb.add_listener(
    "PublicListener",
    port=80,
    open=True
)

health_check = elbv2.HealthCheck(
    interval=Duration.seconds(60),
    path="/health",
    timeout=Duration.seconds(5)
)

# Attach ALB to ECS Service
listener.add_targets(
    "ECS",
    port=80,
    targets=[service],
    health_check=health_check,
)

CfnOutput(
    stack, "LoadBalancerDNS",
    value=lb.load_balancer_dns_name
)

app.synth()