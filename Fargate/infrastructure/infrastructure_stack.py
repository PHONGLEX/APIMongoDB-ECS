import aws_cdk as cdk
from aws_cdk import (
    # Duration,
    Stack,
    # aws_sqs as sqs,
    aws_ecs as ecs, aws_ecr as ecr, aws_ec2 as ec2, aws_iam as iam, aws_ecs_patterns as ecs_patterns
)
from constructs import Construct

class InfrastructureStack(Stack):

    def __init__(self, scope: Construct, construct_id: str, **kwargs) -> None:
        super().__init__(scope, construct_id, **kwargs)

        # Create the ECR Repository
        ecr_repository = ecr.Repository(self,
                                        "ecs-devops-sandbox-repository",
                                        repository_name="ecs-devops-sandbox-repository")

        # Create the ECS Cluster (and VPC)
        vpc = ec2.Vpc(self,
                      "ecs-devops-sandbox-vpc",
                      max_azs=3)

        cluster = ecs.Cluster(self,
                              "ecs-devops-sandbox-cluster",
                              cluster_name="ecs-devops-sandbox-cluster",
                              vpc=vpc)

        # Create the ECS Task Definition with placeholder container (and named Task Execution IAM Role)
        execution_role = iam.Role(self,
                                  "ecs-devops-sandbox-execution-role",
                                  assumed_by=iam.ServicePrincipal("ecs-tasks.amazonaws.com"),
                                  role_name="ecs-devops-sandbox-execution-role")

        execution_role.add_to_policy(iam.PolicyStatement(
            effect=iam.Effect.ALLOW,
            resources=["*"],
            actions=[
                "ecr:GetAuthorizationToken",
                "ecr:BatchCheckLayerAvailability",
                "ecr:GetDownloadUrlForLayer",
                "ecr:BatchGetImage",
                "logs:CreateLogStream",
                "logs:PutLogEvents"              
            ]
        ))
        
        # execution_role.add_to_policy(iam.PolicyStatement(
        #     effect=iam.Effect.ALLOW,
        #     resources=["arn:aws:ssm:us-east-1:134477770615:parameter/test/weather-forecast/*"],
        #     actions=[
        #         "ssm:PutParameter",
        #         "ssm:DeleteParameter",
        #         "ssm:GetParameterHistory",
        #         "ssm:GetParametersByPath",
        #         "ssm:GetParameters",
        #         "ssm:GetParameter",
        #         "ssm:DeleteParameters"          
        #         ]
        # ))
        
        task_role = iam.Role(self, "ecs-devops-sandbox-task-role",
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
                "ssm:DeleteParameters",
                "kms:Decrypt"         
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

        task_definition = ecs.FargateTaskDefinition(self,
                                                    "ecs-devops-sandbox-task-definition",
                                                    execution_role=execution_role,
                                                    task_role=task_role,
                                                    family="ecs-devops-sandbox-task-definition")

        container = task_definition.add_container(
            "ecs-devops-sandbox",
            image=ecs.ContainerImage.from_registry("amazon/amazon-ecs-sample"),
            port_mappings=[ecs.PortMapping(container_port=80)]
        )

        # Create the ECS Service
        fargate_service = ecs_patterns.ApplicationLoadBalancedFargateService(
            self, "ecs-devops-sandbox-service",
            cluster=cluster,
            task_definition=task_definition,
            service_name="ecs-devops-sandbox-service"
        )

        fargate_service.service.connections.security_groups[0].add_ingress_rule(
            peer = ec2.Peer.ipv4(vpc.vpc_cidr_block),
            connection = ec2.Port.tcp(80),
            description="Allow http inbound from VPC"
        )
        
        fargate_service.target_group.configure_health_check(path="/health")

        # Setup AutoScaling policy
        scaling = fargate_service.service.auto_scale_task_count(
            max_capacity=5
        )

        scaling.scale_on_cpu_utilization(
            "CpuScaling",
            target_utilization_percent=70,
            scale_in_cooldown=cdk.Duration.seconds(60),
            scale_out_cooldown=cdk.Duration.seconds(60),
        )

        cdk.CfnOutput(
            self, "LoadBalancerDNS",
            value=fargate_service.load_balancer.load_balancer_dns_name
        )
